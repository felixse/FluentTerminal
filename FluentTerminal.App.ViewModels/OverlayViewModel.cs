using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using System;

namespace FluentTerminal.App.ViewModels
{
    public class OverlayViewModel : ViewModelBase
    {
        private readonly IDispatcherTimer _overlayTimer;
        private bool _showOverlay;
        private string _overlayContent;

        public OverlayViewModel(IDispatcherTimer dispatcherTimer)
        {
            _overlayTimer = dispatcherTimer;
            _overlayTimer.Interval = new TimeSpan(0, 0, 2);
        }

        public bool ShowOverlay
        {
            get => _showOverlay;
            set => Set(ref _showOverlay, value);
        }

        public string OverlayContent
        {
            get => _overlayContent;
            set => Set(ref _overlayContent, value);
        }

        public void Show(string message)
        {
            OverlayContent = message; 
            ShowOverlay = true;

            if (_overlayTimer.IsEnabled)
            {
                _overlayTimer.Stop();
                _overlayTimer.Tick -= OnResizeOverlayTimerFinished;
            }
            _overlayTimer.Start();
            _overlayTimer.Tick += OnResizeOverlayTimerFinished;
        }

        private void OnResizeOverlayTimerFinished(object sender, object e)
        {
            _overlayTimer.Stop();
            _overlayTimer.Tick -= OnResizeOverlayTimerFinished;
            ShowOverlay = false;
        }

    }
}
