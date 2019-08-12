using System.Threading;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    internal class DelayedHistorySaver : ISessionSuccessTracker
    {
        private const int DelayMilliseconds = 5000;

        private readonly CommandProfileProviderViewModel _vm;

        private readonly object _lock = new object();

        private CancellationTokenSource _cts;

        private bool _toSave = true;

        internal DelayedHistorySaver(CommandProfileProviderViewModel vm)
        {
            _vm = vm;
        }

        public void SetSuccessfulSessionStart()
        {
            lock (_lock)
            {
                if (!_toSave)
                {
                    return;
                }

                _cts = new CancellationTokenSource();

                InitiateSave(_cts.Token);
            }
        }

        public void SetExitCode(int exitCode)
        {
            lock (_lock)
            {
                if (!_toSave || !(_cts is CancellationTokenSource cts))
                {
                    return;
                }

                _toSave = exitCode == 0;

                cts.Cancel();
                cts.Dispose();
                _cts = null;
            }
        }

        private void InitiateSave(CancellationToken cancellationToken)
        {
            // ReSharper disable once MethodSupportsCancellation
            Task.Delay(DelayMilliseconds, cancellationToken).ContinueWith(t =>
                {
                    lock (_lock)
                    {
                        if (_toSave)
                        {
                            _vm.SaveToHistory();
                        }

                        _toSave = false;

                        _cts?.Dispose();
                        _cts = null;
                    }
                });
        }
    }
}