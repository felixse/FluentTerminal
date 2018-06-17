using System;
using FluentTerminal.App.Services;
using Windows.UI.Xaml;

namespace FluentTerminal.App.Adapters
{
    public class DispatcherTimerAdapter : IDispatcherTimer
    {
        private readonly DispatcherTimer _dispatcherTimer;

        public DispatcherTimerAdapter()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += OnDispatcherTimerTick;
        }

        private void OnDispatcherTimerTick(object sender, object e)
        {
            Tick?.Invoke(this, null);
        }

        public TimeSpan Interval
        {
            get => _dispatcherTimer.Interval;
            set => _dispatcherTimer.Interval = value;
        }

        public bool IsEnabled => _dispatcherTimer.IsEnabled;

        public event EventHandler<object> Tick;

        public void Start()
        {
            _dispatcherTimer.Start();
        }

        public void Stop()
        {
            _dispatcherTimer.Stop();
        }
    }
}
