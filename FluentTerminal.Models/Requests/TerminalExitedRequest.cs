namespace FluentTerminal.Models.Requests
{
    public class TerminalExitedRequest : IMessage
    {
        public const byte Identifier = 7;

        byte IMessage.Identifier => Identifier;

        public int TerminalId { get; set; }
        public int ExitCode { get; set; }

        public TerminalExitedRequest(int terminalId, int exitCode)
        {
            TerminalId = terminalId;
            ExitCode = exitCode;
        }

        public TerminalExitedRequest(int terminalId)
        {
            TerminalId = terminalId;
            ExitCode = -1;
        }

        // This is for JSON deserialisation.
        public TerminalExitedRequest()
        {
            TerminalId = -1;
            ExitCode = -2;
        }

        // Create a TerminalExitedRequest from a TerminalExitStatus.
        public TerminalExitedRequest(TerminalExitStatus status)
        {
            TerminalId = status.TerminalId;
            ExitCode = status.ExitCode;
        }

        // Convert to a TerminalExitStatus.
        public TerminalExitStatus ToStatus()
        {
            return new TerminalExitStatus(TerminalId, ExitCode);
        }
    }
}