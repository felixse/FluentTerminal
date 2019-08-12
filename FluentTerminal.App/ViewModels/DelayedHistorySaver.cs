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

        private bool _sessionStarted;
        private bool _outputReceived;

        private bool _toSave = true;

        internal DelayedHistorySaver(CommandProfileProviderViewModel vm)
        {
            _vm = vm;
        }

        public void SetSuccessfulSessionStart()
        {
            lock (_lock)
            {
                if (!_toSave || _sessionStarted)
                {
                    return;
                }

                _sessionStarted = true;

                if (_outputReceived)
                {
                    InitiateSave();
                }
            }
        }

        public void SetOutputReceived()
        {
            lock (_lock)
            {
                if (!_toSave || _outputReceived)
                {
                    return;
                }

                _outputReceived = true;

                if (_sessionStarted)
                {
                    InitiateSave();
                }
            }
        }

        public void SetInvalid()
        {
            lock (_lock)
            {
                if (!_toSave || !(_cts is CancellationTokenSource cts))
                {
                    return;
                }

                _toSave = false;

                cts.Cancel();
                cts.Dispose();
                _cts = null;
            }
        }

        private void InitiateSave()
        {
            _cts = new CancellationTokenSource();

            Task.Delay(DelayMilliseconds, _cts.Token).ContinueWith(t =>
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