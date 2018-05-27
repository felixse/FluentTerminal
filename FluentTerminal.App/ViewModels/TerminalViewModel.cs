using FluentTerminal.App.Services;
using FluentTerminal.App.Views;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private ApplicationSettings _applicationSettings;
        private readonly CoreDispatcher _dispatcher;
        private string _resizeOverlayContent;
        private readonly DispatcherTimer _resizeOverlayTimer;
        private bool _showResizeOverlay;
        private readonly string _startupDirectory;
        private int _terminalId;
        private ITerminalView _terminalView;
        private string _title;
        private readonly ShellProfile _shellProfile;
        private bool _showSearchPanel;
        private string _searchText;

        public TerminalViewModel(int id, ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService,
            IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, string startupDirectory, ShellProfile shellProfile)
        {
            Id = id;
            Title = DefaultTitle;

            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _settingsService.KeyBindingsChanged += OnKeyBindingsChanged;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            _applicationSettings = applicationSettings;
            _startupDirectory = startupDirectory;
            _shellProfile = shellProfile;
            _resizeOverlayTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 2)
            };
            _resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;

            CloseCommand = new RelayCommand(async () => await InvokeCloseRequested().ConfigureAwait(false));
            FindNextCommand = new RelayCommand(async () => await FindNext().ConfigureAwait(false));
            FindPreviousCommand = new RelayCommand(async () => await FindPrevious().ConfigureAwait(false));
            CloseSearchPanelCommand = new RelayCommand(CloseSearchPanel);

            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
        }

        private async void OnKeyBindingsChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var keyBindings = _settingsService.GetKeyBindings();
                await _terminalView.ChangeKeyBindings(GetKeyBindingsCollection(keyBindings)).ConfigureAwait(false);
            });
        }

        public event EventHandler CloseRequested;

        public event EventHandler<string> TitleChanged;

        public RelayCommand CloseCommand { get; }

        public RelayCommand FindPreviousCommand { get; }
        public RelayCommand FindNextCommand { get; }
        public RelayCommand CloseSearchPanelCommand { get; }

        public int Id { get; }

        public bool Initialized { get; private set; }

        public string DefaultTitle { get; private set; } = "Fluent Terminal";

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
                value = string.IsNullOrWhiteSpace(value) ? DefaultTitle: value;

                if (Set(ref _title, value))
                {
                    TitleChanged?.Invoke(this, Title);
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

            var size = await _terminalView.CreateTerminal(options, theme.Colors, GetKeyBindingsCollection(keyBindings)).ConfigureAwait(true);

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

                DefaultTitle = response.ShellExecutableName;
                Title = DefaultTitle;

                await _terminalView.ConnectToSocket(response.WebSocketUrl).ConfigureAwait(true);
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

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void OnApplicationSettingsChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _applicationSettings = _settingsService.GetApplicationSettings());
        }

        private async void OnCurrentThemeChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var currentTheme = _settingsService.GetCurrentTheme();
                await _terminalView.ChangeTheme(currentTheme.Colors).ConfigureAwait(true);
            });
        }

        private async void OnKeyboardCommandReceived(object sender, Command e)
        {
            if (e == Command.Copy)
            {
                var selection = await _terminalView.GetSelection().ConfigureAwait(true);
                var dataPackage = new DataPackage();
                dataPackage.SetText(selection);
                Clipboard.SetContent(dataPackage);
            }
            else if (e == Command.Paste)
            {
                var content = Clipboard.GetContent();
                if (content.Contains(StandardDataFormats.Text))
                {
                    var text = await content.GetTextAsync();
                    await _terminalView.Write(text).ConfigureAwait(true);
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

        private async void OnTerminalOptionsChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var options = _settingsService.GetTerminalOptions();
                await _terminalView.ChangeOptions(options).ConfigureAwait(true);
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

        private IEnumerable<KeyBinding> GetKeyBindingsCollection(KeyBindings keyBindings)
        {
            var list = new List<KeyBinding>();
            list.AddRange(keyBindings.CloseTab);
            list.AddRange(keyBindings.Copy);
            list.AddRange(keyBindings.NewTab);
            list.AddRange(keyBindings.ConfigurableNewTab);
            list.AddRange(keyBindings.NewWindow);
            list.AddRange(keyBindings.NextTab);
            list.AddRange(keyBindings.Paste);
            list.AddRange(keyBindings.PreviousTab);
            list.AddRange(keyBindings.ShowSettings);
            list.AddRange(keyBindings.Search);

            return list;
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
    }
}