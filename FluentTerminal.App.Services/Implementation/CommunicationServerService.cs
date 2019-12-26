using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.Models.Messages.Protobuf;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;

namespace FluentTerminal.App.Services.Implementation
{
    public sealed class CommunicationServerService : CommunicationServiceBase, ICommunicationServerService
    {
        // TODO: This should be removed! New implementation must implement binding to any free port.
        public const ushort TempTerminalDataPort = 49382;

        internal const int PubSubHighWatermark = 1000;

        private readonly Queue<TaskCompletionSource<TerminalData>> _queue =
            new Queue<TaskCompletionSource<TerminalData>>();

        public void SendTerminalDataEvent(byte terminalId, byte[] data) => EnqueueTerminalDataEvent(new TerminalData
            {TerminalId = terminalId, Bytes = ByteString.CopyFrom(data)});

        protected override void Runner(ushort port, TaskCompletionSource<ushort> tcsPort)
        {
            using (var pubSocket = new PublisherSocket())
            {
                pubSocket.Options.SendHighWatermark = PubSubHighWatermark;
                if (port > 0)
                    pubSocket.Bind($"tcp://localhost:{port:#####}");
                else
                    port = (ushort)pubSocket.BindRandomPort("tcp://localhost");

                tcsPort.SetResult(port);

                // ReSharper disable once InconsistentlySynchronizedField
                _queue.Enqueue(new TaskCompletionSource<TerminalData>());

                TaskCompletionSource<TerminalData> next = null;

                while (true)
                {
                    lock (Lock)
                    {
                        if (Disposed)
                            return;

                        // Removing the item which is processed in the last cycle
                        if (next != null && next != _queue.Dequeue())
                            throw new Exception("Shouldn't happen ever! Item already dequeued.");

                        next = _queue.Peek();
                    }

                    try
                    {
                        pubSocket.SendFrame(next.Task.Result.ToByteArray());
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        private void EnqueueTerminalDataEvent(TerminalData data)
        {
            lock (Lock)
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(CommunicationServerService));

                var tcs = _queue.LastOrDefault();

                if (tcs == null)
                    throw new InvalidOperationException($"{nameof(Spawn)} method must be called first.");

                _queue.Enqueue(new TaskCompletionSource<TerminalData>());

                tcs.TrySetResult(data);
            }
        }

        public override void Dispose()
        {
            lock (Lock)
            {
                if (Disposed)
                    return;

                Disposed = true;

                while (_queue.Any()) _queue.Dequeue().TrySetCanceled();
            }
        }
    }
}
