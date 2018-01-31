using FluentTerminal.App.Models;
using FluentTerminal.App.Views;
using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Web.Http;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private static HttpClient _httpClient;
        private ITerminalView _terminalView;
        private string _title;
        private string _resizeOverlayContent;
        private bool _showResizeOverlay;
        private DispatcherTimer _resizeOverlayTimer;

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
                if (!Initialized)
                {
                    return;
                }

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

        public TerminalViewModel()
        {
            _resizeOverlayTimer = new DispatcherTimer();
            _resizeOverlayTimer.Interval = new TimeSpan(0, 0, 2);
            _resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;
        }

        private void OnResizeOverlayTimerFinished(object sender, object e)
        {
            _resizeOverlayTimer.Stop();
            ShowResizeOverlay = false;
        }

        static TerminalViewModel()
        {
            _httpClient = new HttpClient();
        }

        public async Task OnViewIsReady(ITerminalView terminalView)
        {
            _terminalView = terminalView;
            _terminalView.TerminalSizeChanged += OnTerminalSizeChanged;
            _terminalView.TerminalTitleChanged += OnTerminalTitleChanged;

            var size = await _terminalView.CreateTerminal(null);

            var response = await _httpClient.PostAsync(new Uri($"http://localhost:9000/terminals?cols={size.Columns}&rows={size.Rows}"), null);
            var url = await response.Content.ReadAsStringAsync();
            Id = int.Parse(url.Split(":")[2].Trim('"'));

            await _terminalView.ConnectToSocket(url);
            Initialized = true;
        }

        private void OnTerminalTitleChanged(object sender, string e)
        {
            Title = e;   
        }

        private async void OnTerminalSizeChanged(object sender, TerminalSize e)
        {
            ResizeOverlayContent = $"{e.Columns} x {e.Rows}";
            ShowResizeOverlay = true;
            await _httpClient.PostAsync(new Uri($"http://localhost:9000/terminals/{Id}/size?cols={e.Columns}&rows={e.Rows}"), null);
        }
    }
}
