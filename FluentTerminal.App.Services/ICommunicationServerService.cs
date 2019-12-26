using System;

namespace FluentTerminal.App.Services
{
    public interface ICommunicationServerService : ICommunicationService
    {
        void SendTerminalDataEvent(Guid terminalId, byte[] data);
    }
}
