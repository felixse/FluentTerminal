namespace FluentTerminal.Models
{
    public class TerminalExitStatus
    {
        public TerminalExitStatus(byte terminalId, int exitCode)
        {
            TerminalId = terminalId;
            ExitCode = exitCode;
        }

        public byte TerminalId { get; set; }
        public int ExitCode { get; set; }
    }
}
