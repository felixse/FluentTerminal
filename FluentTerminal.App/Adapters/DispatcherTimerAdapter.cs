using FluentTerminal.App.Services;
using System;
using Windows.UI.Xaml;

namespace FluentTerminal.App.Adapters
{
    public class DispatcherTimerAdapter : IDispatcherTimer
    {
        private readonly DispatcherTimer _dispatcherTimer;

        public DispatcherTimerAdapter()
        {
            _dispatcherTimer = new DispatcherTimer();
        }

        private void OnDispatcherTimerTick(object sender, object e)
        {
            Tick?.Invoke(this, EventArgs.Empty);
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
            _dispatcherTimer.Tick += OnDispatcherTimerTick;
            _dispatcherTimer.Start();
        }

        public void Stop()
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Tick -= OnDispatcherTimerTick;
        }
    }
}