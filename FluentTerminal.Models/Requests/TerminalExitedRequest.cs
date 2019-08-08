namespace FluentTerminal.Models.Requests
{
    public class TerminalExitedRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.TerminalExitedRequest;

        public byte TerminalId { get; set; }
        public int ExitCode { get; set; }

        public TerminalExitedRequest(byte terminalId, int exitCode)
        {
            TerminalId = terminalId;
            ExitCode = exitCode;
        }

        public TerminalExitedRequest(byte terminalId)
        {
            TerminalId = terminalId;
            ExitCode = -1;
        }

        public TerminalExitedRequest()
        {
        }

        public TerminalExitedRequest(TerminalExitStatus status)
        {
            TerminalId = status.TerminalId;
            ExitCode = status.ExitCode;
        }

        public TerminalExitStatus ToStatus()
        {
            return new TerminalExitStatus(TerminalId, ExitCode);
        }
    }
}