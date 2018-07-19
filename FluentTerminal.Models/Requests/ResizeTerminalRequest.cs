namespace FluentTerminal.Models.Requests
{
    public class ResizeTerminalRequest
    {
        public int TerminalId { get; set; }

        public TerminalSize NewSize { get; set; }
    }
}