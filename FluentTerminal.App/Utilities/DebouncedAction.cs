using System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace FluentTerminal.App.Utilities
{
    public class DebouncedAction<T>
    {
        private DispatcherTimer timer;
        private readonly CoreDispatcher _dispatcher;
        private readonly TimeSpan _interval;
        private Action<T> _action;
        private T _parameter;

        public DebouncedAction(CoreDispatcher dispatcher, TimeSpan interval, Action<T> action)
        {
            _dispatcher = dispatcher;
            _interval = interval;
            _action = action;
        }

        public void Invoke(T parameter)
        {
            _parameter = parameter;

            ResetTimer();

            timer = new DispatcherTimer
            {
                Interval = _interval
            };
            timer.Tick += async (s, e) =>
            {
                if (!ResetTimer())
                {
                    return;
                }

                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _action.Invoke(_parameter);
                });
            };
            timer.Start();
        }

        public void Stop()
        {
            ResetTimer();
            _action = null;
        }

        private bool ResetTimer()
        {
            if (timer == null)
            {
                return false;
            }
            timer.Stop();
            timer = null;
            return true;
        }
    }
}
