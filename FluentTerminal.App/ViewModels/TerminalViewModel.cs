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
        private readonly ITerminalService _terminalService;
        private ApplicationSettings _applicationSettings;
        private CoreDispatcher _dispatcher;
        private string _resizeOverlayContent;
        private DispatcherTimer _resizeOverlayTimer;
        private bool _showResizeOverlay;
        private string _startupDirectory;
        private int _terminalId;
        private ITerminalView _terminalView;
        private string _title;

        public TerminalViewModel(int id, ISettingsService settingsService, ITerminalService terminalService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, string startupDirectory)
        {
            Id = id;
            Title = DefaultTitle;

            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _settingsService.KeyBindingsChanged += OnKeyBindingsChanged;
            _terminalService = terminalService;
            _dialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            _applicationSettings = applicationSettings;
            _startupDirectory = startupDirectory;
            _resizeOverlayTimer = new DispatcherTimer();
            _resizeOverlayTimer.Interval = new TimeSpan(0, 0, 2);
            _resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;

            CloseCommand = new RelayCommand(async () => await InvokeCloseRequested());

            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
        }

        private async void OnKeyBindingsChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var keyBindings = _settingsService.GetKeyBindings();
                await _terminalView.ChangeKeyBindings(GetKeyBindingsCollection(keyBindings));
            });
        }

        public event EventHandler CloseRequested;

        public event EventHandler<string> TitleChanged;

        public RelayCommand CloseCommand { get; }

        public int Id { get; private set; }

        public bool Initialized { get; private set; }

        public string DefaultTitle { get; private set; } = "Fluent Terminal";

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

            var size = await _terminalView.CreateTerminal(options, theme.Colors, GetKeyBindingsCollection(keyBindings));
            var configuration = _settingsService.GetShellConfiguration();

            if (!string.IsNullOrWhiteSpace(_startupDirectory))
            {
                configuration.WorkingDirectory = _startupDirectory;
            }

            var response = await _terminalService.CreateTerminal(size, configuration);

            if (response.Success)
            {
                _terminalId = response.Id;
                _terminalView.TerminalSizeChanged += OnTerminalSizeChanged;
                _terminalView.TerminalTitleChanged += OnTerminalTitleChanged;
                _terminalView.KeyboardCommandReceived += OnKeyboardCommandReceived;

                DefaultTitle = response.ShellExecutableName;
                Title = DefaultTitle;

                await _terminalView.ConnectToSocket(response.WebSocketUrl);
                Initialized = true;
            }
            else
            {
                await _dialogService.ShowDialogAsnyc("Error", response.Error, DialogButton.OK);
            }

            await FocusTerminal();
        }

        private async Task InvokeCloseRequested()
        {
            if (_applicationSettings.ConfirmClosingTabs)
            {
                var result = await _dialogService.ShowDialogAsnyc("Please confirm", "Are you sure you want to close this tab?", DialogButton.OK, DialogButton.Cancel);

                if (result == DialogButton.Cancel)
                {
                    return;
                }
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void OnApplicationSettingsChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _applicationSettings = _settingsService.GetApplicationSettings();
            });
        }

        private async void OnCurrentThemeChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var currentTheme = _settingsService.GetCurrentTheme();
                await _terminalView.ChangeTheme(currentTheme.Colors);
            });
        }

        private async void OnKeyboardCommandReceived(object sender, Command e)
        {
            if (e == Command.Copy)
            {
                var selection = await _terminalView.GetSelection();
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
                    await _terminalView.Write(text);
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
                await _terminalView.ChangeOptions(options);
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
            await _terminalService.ResizeTerminal(_terminalId, e);
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
            list.AddRange(keyBindings.NewWindow);
            list.AddRange(keyBindings.NextTab);
            list.AddRange(keyBindings.Paste);
            list.AddRange(keyBindings.PreviousTab);
            list.AddRange(keyBindings.ShowSettings);

            return list;
        }
    }
}