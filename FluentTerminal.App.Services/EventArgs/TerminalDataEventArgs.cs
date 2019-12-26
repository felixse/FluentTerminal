using System;

namespace FluentTerminal.App.Services.EventArgs
{
    public class TerminalDataEventArgs : TerminalEventArgs
    {
        public byte[] Data { get; }
        
        public TerminalDataEventArgs(Guid terminalId, byte[] data) : base(terminalId) => Data = data;
    }
}
