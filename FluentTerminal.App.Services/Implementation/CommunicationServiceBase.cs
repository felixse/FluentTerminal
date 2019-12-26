using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Implementation
{
    public abstract class CommunicationServiceBase : ICommunicationService
    {
        protected readonly object Lock = new object();

        protected bool Disposed;

        public ushort PubSubPort { get; private set; }

        public ushort Spawn(ushort port)
        {
            lock (Lock)
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(CommunicationServerService));

                if (PubSubPort > 0)
                    throw new InvalidOperationException("Already spawned.");

                TaskCompletionSource<ushort> tcsPort = new TaskCompletionSource<ushort>();

                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = Task.Factory.StartNew(() => Runner(port, tcsPort));

                PubSubPort = tcsPort.Task.Result;

                return PubSubPort;
            }
        }

        public virtual void Dispose()
        {
            lock (Lock)
            {
                Disposed = true;
            }
        }

        protected abstract void Runner(ushort port, TaskCompletionSource<ushort> tcsPort);
    }
}
