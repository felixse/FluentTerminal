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
        private readonly WeakReference<Action<T>> _action;
        private T _parameter;

        public DebouncedAction(CoreDispatcher dispatcher, TimeSpan interval, Action<T> action)
        {
            _dispatcher = dispatcher;
            _interval = interval;
            _action = new WeakReference<Action<T>>(action);
        }

        public void Invoke(T parameter)
        {
            _parameter = parameter;

            timer?.Stop();
            timer = null;

            timer = new DispatcherTimer
            {
                Interval = _interval
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (timer == null)
            {
                return;
            }

            timer?.Stop();
            timer.Tick -= Timer_Tick;
            timer = null;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_action.TryGetTarget(out Action<T> target))
                {
                    target.Invoke(_parameter);
                }
            });
        }
    }
}
