using Fleck;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private ApplicationSettings _applicationSettings;
        private string _resizeOverlayContent;
        private bool _showResizeOverlay;
        private readonly string _startupDirectory;
        private int _terminalId;
        private ITerminalView _terminalView;
        private string _title;
        private readonly ShellProfile _shellProfile;
        private bool _showSearchPanel;
        private string _searchText;
        private bool _isSelected;
        private readonly IApplicationView _applicationView;
        private readonly IDispatcherTimer _resizeOverlayTimer;
        private readonly IClipboardService _clipboardService;
        private IWebSocketConnection _webSocket;
        private ManualResetEventSlim _connectedEvent;

        public TerminalViewModel(int id, ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService,
            IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, string startupDirectory, ShellProfile shellProfile,
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService)
        {
            Id = id;
            Title = DefaultTitle;

            _connectedEvent = new ManualResetEventSlim(false);

            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _settingsService.KeyBindingsChanged += OnKeyBindingsChanged;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _trayProcessCommunicationService.TerminalExited += OnTerminalExited;
            _dialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            _applicationSettings = applicationSettings;
            _startupDirectory = startupDirectory;
            _shellProfile = shellProfile;
            _applicationView = applicationView;
            _clipboardService = clipboardService;
            _resizeOverlayTimer = dispatcherTimer;
            _resizeOverlayTimer.Interval = new TimeSpan(0, 0, 2);
            _resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;

            CloseCommand = new RelayCommand(async () => await InvokeCloseRequested().ConfigureAwait(false));
            FindNextCommand = new RelayCommand(async () => await FindNext().ConfigureAwait(false));
            FindPreviousCommand = new RelayCommand(async () => await FindPrevious().ConfigureAwait(false));
            CloseSearchPanelCommand = new RelayCommand(CloseSearchPanel);
            
        }

        private void OnTerminalExited(object sender, int e)
        {
            if (e == _terminalId)
            {
                _applicationView.RunOnDispatcherThread(() => CloseRequested?.Invoke(this, EventArgs.Empty));
            }
        }

        private async void OnKeyBindingsChanged(object sender, EventArgs e)
        {
            var keyBindings = _settingsService.GetKeyBindings();
            await _applicationView.RunOnDispatcherThread(async () =>
            {
                await _terminalView.ChangeKeyBindings(FlattenKeyBindings(keyBindings)).ConfigureAwait(false);
            });
        }

        public event EventHandler CloseRequested;

        public RelayCommand CloseCommand { get; }

        public RelayCommand FindPreviousCommand { get; }
        public RelayCommand FindNextCommand { get; }
        public RelayCommand CloseSearchPanelCommand { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (Set(ref _isSelected, value))
                {
                    if (IsSelected)
                    {
                        _applicationView.Title = Title;
                    }
                    RaisePropertyChanged(nameof(IsUnderlined));
                }
            }
        }

        public bool IsUnderlined => IsSelected && _applicationSettings.UnderlineSelectedTab;

        public int Id { get; }

        public bool Initialized { get; private set; }

        public string DefaultTitle { get; private set; } = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public bool ShowSearchPanel
        {
            get => _showSearchPanel;
            set => Set(ref _showSearchPanel, value);
        }

        public string ResizeOverlayContent
        {
            get => _resizeOverlayContent;
            set => Set(ref _resizeOverlayContent, value);
        }

        public bool ShowResizeOverlay
        {
            get => _showResizeOverlay;
            set
            {
                Set(ref _showResizeOverlay, value);
                if (value)
                {
                    if (_resizeOverlayTimer.IsEnabled)
                    {
                        _resizeOverlayTimer.Stop();
                    }
                    _resizeOverlayTimer.Start();
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                value = string.IsNullOrWhiteSpace(value) ? DefaultTitle : value;

                if (Set(ref _title, value))
                {
                    if (IsSelected)
                    {
                        _applicationView.Title = Title;
                    }
                }
            }
        }

        public void CloseView()
        {
            _terminalView.Close();
        }

        public Task FocusTerminal()
        {
            return _terminalView?.FocusTerminal();
        }

        public async Task OnViewIsReady(ITerminalView terminalView)
        {
            _terminalView = terminalView;

            var options = _settingsService.GetTerminalOptions();
            var theme = _settingsService.GetCurrentTheme();
            var keyBindings = _settingsService.GetKeyBindings();

            var size = await _terminalView.CreateTerminal(options, theme.Colors, FlattenKeyBindings(keyBindings)).ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(_startupDirectory))
            {
                _shellProfile.WorkingDirectory = _startupDirectory;
            }

            var response = await _trayProcessCommunicationService.CreateTerminal(size, _shellProfile).ConfigureAwait(true);

            if (response.Success)
            {
                _terminalId = response.Id;
                _terminalView.TerminalSizeChanged += OnTerminalSizeChanged;
                _terminalView.TerminalTitleChanged += OnTerminalTitleChanged;
                _terminalView.KeyboardCommandReceived += OnKeyboardCommandReceived;

                _trayProcessCommunicationService.SubscribeForTerminalOutput(_terminalId, t =>
                {
                        _connectedEvent.Wait();
                        _webSocket.Send(t);
                });

                DefaultTitle = response.ShellExecutableName;
                Title = DefaultTitle;

                var webSocketUrl = "ws://127.0.0.1:" + response.Id;
                var webSocketServer = new WebSocketServer(webSocketUrl);
                webSocketServer.Start(socket =>
                {
                    _webSocket = socket;
                    socket.OnOpen = () => _connectedEvent.Set();
                    socket.OnClose = () =>
                    {
                        Console.WriteLine("closing");
                    };
                    socket.OnMessage = message =>
                    {
                        _trayProcessCommunicationService.WriteText(_terminalId, message);
                    };
                });

                await _terminalView.ConnectToSocket(webSocketUrl).ConfigureAwait(true);
                Initialized = true;
            }
            else
            {
                await _dialogService.ShowMessageDialogAsnyc("Error", response.Error, DialogButton.OK).ConfigureAwait(true);
            }

            await FocusTerminal().ConfigureAwait(true);
        }

        private async Task InvokeCloseRequested()
        {
            if (_applicationSettings.ConfirmClosingTabs)
            {
                var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to close this tab?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                if (result == DialogButton.Cancel)
                {
                    return;
                }
            }

            await _trayProcessCommunicationService.CloseTerminal(_terminalId).ConfigureAwait(true);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void OnApplicationSettingsChanged(object sender, ApplicationSettings e)
        {
            await _applicationView.RunOnDispatcherThread(() =>
            {
                _applicationSettings = e;
                RaisePropertyChanged(nameof(IsUnderlined));
            });
        }

        private async void OnCurrentThemeChanged(object sender, Guid e)
        {
            await _applicationView.RunOnDispatcherThread(async () =>
            {
                var currentTheme = _settingsService.GetTheme(e);
                await _terminalView.ChangeTheme(currentTheme.Colors).ConfigureAwait(true);
            });
        }

        private async void OnKeyboardCommandReceived(object sender, Command e)
        {
            if (e == Command.Copy)
            {
                var selection = await _terminalView.GetSelection().ConfigureAwait(true);
                _clipboardService.SetText(selection);
            }
            else if (e == Command.Paste)
            {
                var content = await _clipboardService.GetText().ConfigureAwait(true);
                if (content != null)
                {
                    await _trayProcessCommunicationService.WriteText(_terminalId, content).ConfigureAwait(true);
                }
            }
            else if (e == Command.Search)
            {
                ShowSearchPanel = !ShowSearchPanel;
                if (ShowSearchPanel)
                {
                    _terminalView.FocusSearchTextBox();
                }
            }
            else
            {
                _keyboardCommandService.SendCommand(e);
            }
        }

        private void OnResizeOverlayTimerFinished(object sender, object e)
        {
            _resizeOverlayTimer.Stop();
            ShowResizeOverlay = false;
        }

        private async void OnTerminalOptionsChanged(object sender, TerminalOptions e)
        {
            await _applicationView.RunOnDispatcherThread(async () =>
            {
                await _terminalView.ChangeOptions(e).ConfigureAwait(true);
            });
        }

        private async void OnTerminalSizeChanged(object sender, TerminalSize e)
        {
            if (!Initialized)
            {
                return;
            }
            ResizeOverlayContent = $"{e.Columns} x {e.Rows}";
            ShowResizeOverlay = true;
            await _trayProcessCommunicationService.ResizeTerminal(_terminalId, e).ConfigureAwait(true);
        }

        private void OnTerminalTitleChanged(object sender, string e)
        {
            Title = e;
        }

        private IEnumerable<KeyBinding> FlattenKeyBindings(IDictionary<Command, ICollection<KeyBinding>> keyBindings)
        {
            return keyBindings.Values.SelectMany(k => k);
        }

        private Task FindNext()
        {
            return _terminalView.FindNext(SearchText);
        }

        private Task FindPrevious()
        {
            return _terminalView.FindPrevious(SearchText);
        }

        private void CloseSearchPanel()
        {
            SearchText = string.Empty;
            ShowSearchPanel = false;
            _terminalView.FocusTerminal();
        }

        private const int FirstDynamicPort = 49151;

        public static int? GetAvailablePort()
        {
            var usedPorts = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            var connections = properties.GetActiveTcpConnections();
            usedPorts.AddRange(connections.Where(c => c.LocalEndPoint.Port >= FirstDynamicPort).Select(c => c.LocalEndPoint.Port));

            var endPoints = properties.GetActiveTcpListeners();
            usedPorts.AddRange(endPoints.Where(e => e.Port >= FirstDynamicPort).Select(e => e.Port));

            endPoints = properties.GetActiveUdpListeners();
            usedPorts.AddRange(endPoints.Where(e => e.Port >= FirstDynamicPort).Select(e => e.Port));

            usedPorts.Sort();

            for (var i = FirstDynamicPort; i < UInt16.MaxValue; i++)
            {
                if (!usedPorts.Contains(i))
                {
                    return i;
                }
            }
            return null;
        }
    }
}