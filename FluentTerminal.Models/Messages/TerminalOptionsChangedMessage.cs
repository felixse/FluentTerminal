using System;

namespace FluentTerminal.Models.Messages
{
    public class TerminalOptionsChangedMessage
    {
        public TerminalOptions TerminalOptions { get; }

        public TerminalOptionsChangedMessage(TerminalOptions terminalOptions)
        {
            TerminalOptions = terminalOptions ?? throw new ArgumentNullException();
        }
    }
}