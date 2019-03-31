using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.App.ViewModels
{
    public class OverlayViewModel : ViewModelBase
    {
        private readonly IDispatcherTimer _resizeOverlayTimer;
        private bool _showResizeOverlay = true;
        private string _resizeOverlayContent = "Test";

        public OverlayViewModel()
        {
            //_resizeOverlayTimer = dispatcherTimer;
            //_resizeOverlayTimer.Interval = new TimeSpan(0, 0, 2);
            //_resizeOverlayTimer.Tick += OnResizeOverlayTimerFinished;
            //UpdateOverlay = new RelayCommand<string>(UpdateOverlayText);
        }

        public RelayCommand<string> UpdateOverlay { get; set; }

        public delegate void ParameterChange(object sender, TerminalSize e);

        public ParameterChange OnParameterChange { get; set; }

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

        private void OnResizeOverlayTimerFinished(object sender, object e)
        {
            _resizeOverlayTimer.Stop();
            ShowResizeOverlay = false;
        }

        private void UpdateOverlayText(string test)
        {
            throw new NotImplementedException();
        }

    }
}
