using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml;

namespace FluentTerminal.App.ViewModels
{
    public class OverlayViewModel : ObservableObject
    {
        private readonly DispatcherTimer _overlayTimer;
        private bool _showOverlay;
        private string _overlayContent;

        // Important! The constructor has to be called from the UI thread.
        public OverlayViewModel()
        {
            _overlayTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 2)};
        }

        public bool ShowOverlay
        {
            get => _showOverlay;
            set => SetProperty(ref _showOverlay, value);
        }

        public string OverlayContent
        {
            get => _overlayContent;
            set => SetProperty(ref _overlayContent, value);
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
