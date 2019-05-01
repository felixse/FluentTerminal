using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.Models
{
    public class TerminalExitStatus
    {
        public TerminalExitStatus(int terminalId, int exitCode)
        {
            TerminalId = terminalId;
            ExitCode = exitCode;
        }

        public int TerminalId { get; set; }
        public int ExitCode { get; set; }
    }
}
