using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace FluentTerminal.App.Utilities
{
    public class DelayedAction<T> : IDisposable
    {
        private readonly object _lock = new object();

        private readonly Action<T> _action;
        private readonly int _delayMilliseconds;
        private readonly CoreDispatcher _dispatcher;

        private CancellationTokenSource _cts;

        public DelayedAction(Action<T> action, int delayMilliseconds, CoreDispatcher dispatcher = null)
        {
            _action = action;
            _delayMilliseconds = delayMilliseconds;
            _dispatcher = dispatcher;
        }

        public Task InvokeAsync(T parameter)
        {
            CancellationTokenSource cts;

            lock (_lock)
            {
                try
                {
                    _cts?.Cancel();
                }
                catch
                {
                    // ignored
                }

                _cts = cts = new CancellationTokenSource();
            }

            return Task.Delay(_delayMilliseconds, cts.Token).ContinueWith(async t =>
            {
                if (!cts.IsCancellationRequested)
                {
                    if (_dispatcher == null)
                    {
                        _action(parameter);
                    }
                    else
                    {
                        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _action(parameter));
                    }
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public void Cancel()
        {
            lock (_lock)
            {
                try
                {
                    _cts?.Cancel();
                }
                catch
                {
                    // ignored
                }

                _cts?.Dispose();
            }
        }

        public void Dispose() => Cancel();
    }
}