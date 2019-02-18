﻿using Fleck;
using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.App.ViewModels.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private readonly IApplicationView _applicationView;
        private readonly IClipboardService _clipboardService;
        private readonly ManualResetEventSlim _connectedEvent;
        private readonly IDialogService _dialogService;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly IDispatcherTimer _resizeOverlayTimer;
        private readonly ISettingsService _settingsService;
        private readonly ShellProfile _shellProfile;
        private readonly string _startupDirectory;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private InputBuffer _buffer;
        private bool _isSelected;
        private bool _newOutput;
        private string _resizeOverlayContent;
        private string _searchText;
        private bool _showResizeOverlay;
        private bool _showSearchPanel;
        private TabTheme _tabTheme;
        private int _terminalId;
        private TerminalTheme _terminalTheme;
        private ITerminalView _terminalView;
        private string _title;
        private IWebSocketConnection _webSocket;

        public TerminalViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService,
            IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, string startupDirectory, ShellProfile shellProfile,
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService)
        {
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
            ApplicationSettings = applicationSettings;
            _startupDirectory = startupDirectory;
            _shellProfile = shellProfile;
            _applicationView = applicationView;
            _clipboardService = clipboardService;

            TerminalTheme = shellProfile.TerminalThemeId == Guid.Empty ? _settingsService.GetCurrentTheme() : _settingsService.GetTheme(shellProfile.TerminalThemeId);

            TabThemes = new ObservableCollection<TabTheme>(_settingsService.GetTabThemes());
            TabTheme = TabThemes.FirstOrDefault(t => t.Id == _shellProfile.TabThemeId);

            _resizeOverlayTimer = dispatcherTimer;
            _resizeOverlayTimer.Interval = new TimeSpan(0, 0, 2);
            _resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;

            CloseCommand = new RelayCommand(async () => await TryClose().ConfigureAwait(false));
            FindNextCommand = new RelayCommand(async () => await FindNext().ConfigureAwait(false));
            FindPreviousCommand = new RelayCommand(async () => await FindPrevious().ConfigureAwait(false));
            CloseSearchPanelCommand = new RelayCommand(CloseSearchPanel);
            SelectTabThemeCommand = new RelayCommand<string>(SelectTabTheme);
            EditTitleCommand = new AsyncCommand(EditTitle);
        }

        public event EventHandler Closed;

        public ApplicationSettings ApplicationSettings { get; private set; }

        public TabTheme BackgroundTabTheme
        {
            // The effective background theme depends on whether it is selected (use the theme), or if it is inactive
            // (if we're set to underline inactive tabs, use the null theme).
            get => IsSelected || (!IsSelected && ApplicationSettings.InactiveTabColorMode == InactiveTabColorMode.Background) ?
                _tabTheme :
                _settingsService.GetTabThemes().FirstOrDefault(t => t.Color == null);
        }

        public RelayCommand CloseCommand { get; }

        public RelayCommand CloseSearchPanelCommand { get; }

        public string DefaultTitle { get; private set; } = string.Empty;

        public IAsyncCommand EditTitleCommand { get; }

        public RelayCommand FindNextCommand { get; }

        public RelayCommand FindPreviousCommand { get; }

        public bool Initialized { get; private set; }

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
                        _applicationView.RunOnDispatcherThread(() => NewOutput = false);
                    }
                    RaisePropertyChanged(nameof(IsUnderlined));
                    RaisePropertyChanged(nameof(BackgroundTabTheme));
                }
            }
        }

        public bool IsUnderlined => (IsSelected && ApplicationSettings.UnderlineSelectedTab) ||
            (!IsSelected && ApplicationSettings.InactiveTabColorMode == InactiveTabColorMode.Underlined && TabTheme.Color != null);

        public bool NewOutput
        {
            get => _newOutput;
            set => Set(ref _newOutput, value);
        }

        public string ResizeOverlayContent
        {
            get => _resizeOverlayContent;
            set => Set(ref _resizeOverlayContent, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public RelayCommand<string> SelectTabThemeCommand { get; }

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

        public bool ShowSearchPanel
        {
            get => _showSearchPanel;
            set => Set(ref _showSearchPanel, value);
        }

        public TabTheme TabTheme
        {
            get => _tabTheme;
            set
            {
                Set(ref _tabTheme, value);
                RaisePropertyChanged(nameof(IsUnderlined));
                RaisePropertyChanged(nameof(BackgroundTabTheme));
            }
        }

        public ObservableCollection<TabTheme> TabThemes { get; }

        public TerminalTheme TerminalTheme
        {
            get => _terminalTheme;
            set => Set(ref _terminalTheme, value);
        }

        public string Title
        {
            get => _title;
            set
            {
                value = string.IsNullOrWhiteSpace(value) ? DefaultTitle : value;

                if (Set(ref _title, value) && IsSelected)
                {
                    _applicationView.Title = Title;
                }
            }
        }

        public async Task CloseView()
        {
            _terminalView.Close();
            await _trayProcessCommunicationService.CloseTerminal(_terminalId).ConfigureAwait(true);
        }

        public void CopyText(string text)
        {
            _clipboardService.SetText(text);
        }

        public Task FocusTerminal()
        {
            return _terminalView?.FocusTerminal();
        }

        public async Task EditTitle()
        {
            var result = await _dialogService.ShowInputDialogAsync("Edit Title");
            if (result != null)
            {
                Title = result;
            }
        }

        public async Task OnViewIsReady(ITerminalView terminalView)
        {
            _terminalView = terminalView;

            var options = _settingsService.GetTerminalOptions();
            var keyBindings = _settingsService.GetCommandKeyBindings();
            var profiles = _settingsService.GetShellProfiles();

            var settings = _settingsService.GetApplicationSettings();
            var sessionType = SessionType.Unknown;
            if (!_applicationView.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7) || settings.AlwaysUseWinPty)
            {
                sessionType = SessionType.WinPty;
            }
            else
            {
                sessionType = SessionType.ConPty;
            }

            var size = await _terminalView.CreateTerminal(options, TerminalTheme.Colors, FlattenKeyBindings(keyBindings, profiles), sessionType).ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(_startupDirectory))
            {
                _shellProfile.WorkingDirectory = _startupDirectory;
            }

            var response = await _trayProcessCommunicationService.CreateTerminal(size, _shellProfile, sessionType).ConfigureAwait(true);

            if (response.Success)
            {
                _terminalId = response.Id;
                _terminalView.TerminalSizeChanged += OnTerminalSizeChanged;
                _terminalView.TerminalTitleChanged += OnTerminalTitleChanged;
                _terminalView.KeyboardCommandReceived += OnKeyboardCommandReceived;

                _trayProcessCommunicationService.SubscribeForTerminalOutput(_terminalId, t =>
                {
                    _connectedEvent.Wait();
                    _buffer.Write(t);
                    if (!IsSelected && ApplicationSettings.ShowNewOutputIndicator)
                    {
                        _applicationView.RunOnDispatcherThread(() => NewOutput = true);
                    }
                });

                DefaultTitle = response.ShellExecutableName;
                Title = DefaultTitle;

                var webSocketUrl = "ws://127.0.0.1:" + response.Id;
                var webSocketServer = new WebSocketServer(webSocketUrl);
                webSocketServer.Start(socket =>
                {
                    _webSocket = socket;
                    _buffer = new InputBuffer(_webSocket.Send);
                    socket.OnOpen = () => _connectedEvent.Set();
                    socket.OnMessage = message => _trayProcessCommunicationService.WriteText(_terminalId, message);
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

        private void CloseSearchPanel()
        {
            SearchText = string.Empty;
            ShowSearchPanel = false;
            _terminalView.FocusTerminal();
        }

        private Task FindNext()
        {
            return _terminalView.FindNext(SearchText);
        }

        private Task FindPrevious()
        {
            return _terminalView.FindPrevious(SearchText);
        }

        private IEnumerable<KeyBinding> FlattenKeyBindings(IDictionary<string, ICollection<KeyBinding>> commandKeyBindings, IEnumerable<ShellProfile> profiles)
        {
            return commandKeyBindings.Values.SelectMany(k => k).Concat(profiles.SelectMany(x => x.KeyBindings));
        }

        private async void OnApplicationSettingsChanged(object sender, ApplicationSettings e)
        {
            await _applicationView.RunOnDispatcherThread(() =>
            {
                ApplicationSettings = e;
                RaisePropertyChanged(nameof(IsUnderlined));
                RaisePropertyChanged(nameof(BackgroundTabTheme));
            });
        }

        private async void OnCurrentThemeChanged(object sender, Guid e)
        {
            await _applicationView.RunOnDispatcherThread(async () =>
            {
                // only change theme if not overwritten by profile
                if (_shellProfile.TerminalThemeId == Guid.Empty)
                {
                    var currentTheme = _settingsService.GetTheme(e);
                    TerminalTheme = currentTheme;
                    await _terminalView.ChangeTheme(currentTheme.Colors).ConfigureAwait(true);
                }
            });
        }

        private async void OnKeyBindingsChanged(object sender, EventArgs e)
        {
            var keyBindings = _settingsService.GetCommandKeyBindings();
            var profiles = _settingsService.GetShellProfiles();
            await _applicationView.RunOnDispatcherThread(async () =>
            {
                await _terminalView.ChangeKeyBindings(FlattenKeyBindings(keyBindings, profiles)).ConfigureAwait(false);
            });
        }

        private async void OnKeyboardCommandReceived(object sender, string command)
        {
            switch (command)
            {
                case nameof(Command.Copy):
                    {
                        var selection = await _terminalView.GetSelection().ConfigureAwait(true);
                        _clipboardService.SetText(selection);
                        break;
                    }
                case nameof(Command.Paste):
                    {
                        string content = await _clipboardService.GetText().ConfigureAwait(true);
                        if (content != null)
                        {
                            content = _shellProfile.TranslateLineEndings(content);
                            await _trayProcessCommunicationService.WriteText(_terminalId, content).ConfigureAwait(true);
                        }
                        break;
                    }
                case nameof(Command.PasteWithoutNewlines):
                    {
                        string content = await _clipboardService.GetText().ConfigureAwait(true);
                        if (content != null)
                        {
                            content = ShellProfile.NewlinePattern.Replace(content, string.Empty);
                            await _trayProcessCommunicationService.WriteText(_terminalId, content).ConfigureAwait(true);
                        }
                        break;
                    }
                case nameof(Command.Search):
                    {
                        ShowSearchPanel = !ShowSearchPanel;
                        if (ShowSearchPanel)
                        {
                            _terminalView.FocusSearchTextBox();
                        }
                        break;
                    }
                default:
                    {
                        _keyboardCommandService.SendCommand(command);
                        break;
                    }
            }
        }

        private void OnResizeOverlayTimerFinished(object sender, object e)
        {
            _resizeOverlayTimer.Stop();
            ShowResizeOverlay = false;
        }

        private void OnTerminalExited(object sender, int e)
        {
            if (e == _terminalId)
            {
                _applicationView.RunOnDispatcherThread(() => Closed?.Invoke(this, EventArgs.Empty));
            }
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

        private void SelectTabTheme(string name)
        {
            TabTheme = TabThemes.FirstOrDefault(t => t.Name == name);
        }

        private async Task TryClose()
        {
            if (ApplicationSettings.ConfirmClosingTabs)
            {
                var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to close this tab?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                if (result == DialogButton.Cancel)
                {
                    return;
                }
            }

            await _trayProcessCommunicationService.CloseTerminal(_terminalId).ConfigureAwait(true);
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}