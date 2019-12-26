using System;

namespace FluentTerminal.App.Services.EventArgs
{
    public class TerminalEventArgs : System.EventArgs
    {
        public Guid TerminalId { get; }

        public TerminalEventArgs(Guid terminalId) => TerminalId = terminalId;
    }
}