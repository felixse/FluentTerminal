using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models.Requests
{
    public class CreateTerminalRequest
    {
        public int Id { get; set; }
        public TerminalSize Size { get; set; }
        public ShellProfile Profile { get; set; }
        public SessionType SessionType { get; set; }
    }
}