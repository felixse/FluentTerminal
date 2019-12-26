using System;

namespace FluentTerminal.App.Services
{
    public interface ICommunicationServerService : ICommunicationService
    {
        void SendTerminalDataEvent(byte terminalId, byte[] data);
    }
}
