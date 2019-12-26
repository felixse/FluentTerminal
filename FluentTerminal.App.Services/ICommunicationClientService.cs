using System;
using FluentTerminal.App.Services.EventArgs;

namespace FluentTerminal.App.Services
{
    public interface ICommunicationClientService : ICommunicationService
    {
        event EventHandler<TerminalDataEventArgs> TerminalDataReceived;
    }
}
