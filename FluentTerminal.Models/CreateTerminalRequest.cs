namespace FluentTerminal.Models
{
    public class CreateTerminalRequest
    {
        public TerminalSize Size { get; set; }
        public ShellConfiguration Configuration { get; set; }
    }
}
