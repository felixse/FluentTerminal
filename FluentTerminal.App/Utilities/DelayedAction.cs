using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace FluentTerminal.App.Utilities
{
    public class DelayedAction<T> : IDisposable
    {
        private const double DelayThresholdMilliseconds = 5;

        private readonly object _lock = new object();

        private readonly Action<T> _action;
        private readonly int _delayMilliseconds;
        private readonly CoreDispatcher _dispatcher;

        private TaskCompletionSource<object> _tcs;
        private CancellationTokenSource _cts;
        private DateTime _executionTime;
        private T _param;

        public DelayedAction(Action<T> action, int delayMilliseconds, CoreDispatcher dispatcher = null)
        {
            _action = action;
            _delayMilliseconds = delayMilliseconds;
            _dispatcher = dispatcher;
        }

        public Task InvokeAsync(T parameter)
        {
            lock (_lock)
            {
                // Canceling previous (if any)
                _tcs?.TrySetCanceled();

                // Scheduling new
                _tcs = new TaskCompletionSource<object>();
                _param = parameter;
                _executionTime = DateTime.UtcNow.AddMilliseconds(_delayMilliseconds);

                // Launching the loop if not already launched
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();

                    var unused = Loop();
                }

                return _tcs.Task;
            }
        }

        private async Task Loop()
        {
            while (true)
            {
                TimeSpan delay;
                var param = default(T);
                CancellationTokenSource cts;
                TaskCompletionSource<object> tcs = null;

                lock (_lock)
                {
                    cts = _cts;

                    if (cts?.IsCancellationRequested ?? true)
                    {
                        _tcs?.TrySetCanceled();
                        _tcs = null;
                        return;
                    }

                    delay = _executionTime.Subtract(DateTime.UtcNow);

                    // To avoid waiting for only few milliseconds we're using some threshold
                    if (delay.TotalMilliseconds < DelayThresholdMilliseconds)
                    {
                        param = _param;

                        _cts?.Dispose();
                        _cts = cts = null;
                        tcs = _tcs;
                        _tcs = null;
                    }
                }

                if (cts == null)
                {
                    if (_dispatcher == null)
                    {
                        _action(param);
                    }
                    else
                    {
                        await _dispatcher.ExecuteAsync(() => _action(param)).ConfigureAwait(false);
                    }

                    tcs?.TrySetResult(null);

                    return;
                }

                try
                {
                    await Task.Delay(delay, cts.Token).ConfigureAwait(false);
                }
                catch
                {
                    // Canceled
                    break;
                }
            }
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
                _cts = null;

                _tcs?.TrySetCanceled();
                _tcs = null;
            }
        }

        public void Dispose() => Cancel();
    }
}