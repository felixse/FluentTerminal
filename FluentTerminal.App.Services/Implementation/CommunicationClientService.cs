using System;
using System.Threading.Tasks;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.Models;
using FluentTerminal.Models.Messages.Protobuf;
using NetMQ;
using NetMQ.Sockets;

namespace FluentTerminal.App.Services.Implementation
{
    public sealed class CommunicationClientService : CommunicationServiceBase, ICommunicationClientService
    {
        private static readonly TimeSpan ReceivePeriod = TimeSpan.FromMilliseconds(200);

        public event EventHandler<TerminalDataEventArgs> TerminalDataReceived;

        protected override void Runner(ushort port, TaskCompletionSource<ushort> tcsPort)
        {
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.SendHighWatermark = CommunicationServerService.PubSubHighWatermark;
                subSocket.Connect($"tcp://localhost:{port:#####}");
                subSocket.SubscribeToAnyTopic();

                tcsPort.SetResult(port);

                while (true)
                {
                    if (Disposed)
                        return;

                    TerminalData data;

                    try
                    {
                        if (subSocket.TryReceiveFrameBytes(ReceivePeriod, out var bytes))
                            data = TerminalData.Parser.ParseFrom(bytes);
                        else
                            continue;
                    }
                    catch
                    {
                        return;
                    }

                    try
                    {
                        TerminalDataReceived?.Invoke(this, new TerminalDataEventArgs(data.Guid.ToGuid(), data.Bytes.ToByteArray()));
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
