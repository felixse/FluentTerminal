using System;

namespace FluentTerminal.App.Services
{
    public interface ICommunicationService : IDisposable
    {
        ushort PubSubPort { get; }

        ushort Spawn(ushort port);
    }
}
