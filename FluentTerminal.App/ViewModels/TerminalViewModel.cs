using FluentTerminal.App.Services;
using FluentTerminal.App.Views;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private ITerminalView _terminalView;
        private string _title;
        private string _resizeOverlayContent;
        private bool _showResizeOverlay;
        private DispatcherTimer _resizeOverlayTimer;
        private readonly ISettingsService _settingsService;
        private readonly ITerminalService _terminalService;
        private int _terminalId;
        private string _startupDirectory;

        public int Id { get; private set; }

        public bool Initialized { get; private set; }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
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

        public string ResizeOverlayContent
        {
            get => _resizeOverlayContent;
            set => Set(ref _resizeOverlayContent, value);
        }

        public TerminalViewModel(ISettingsService settingsService, ITerminalService terminalService, string startupDirectory)
        {
            _settingsService = settingsService;
            _terminalService = terminalService;
            _startupDirectory = startupDirectory;
            _resizeOverlayTimer = new DispatcherTimer();
            _resizeOverlayTimer.Interval = new TimeSpan(0, 0, 2);
            _resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;
        }

        private void OnResizeOverlayTimerFinished(object sender, object e)
        {
            _resizeOverlayTimer.Stop();
            ShowResizeOverlay = false;
        }

        public async Task OnViewIsReady(ITerminalView terminalView)
        {
            _terminalView = terminalView;

            var theme = _settingsService.GetCurrentThemeColors();

            var size = await _terminalView.CreateTerminal(theme);
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

                await _terminalView.ConnectToSocket(response.WebSocketUrl);
                Initialized = true;
            }
            else
            {
                var dialog = new MessageDialog(response.Error, "Error");
                await dialog.ShowAsync();
            }
        }

        private void OnTerminalTitleChanged(object sender, string e)
        {
            Title = e;   
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
    }
}
