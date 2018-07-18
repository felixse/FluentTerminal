namespace FluentTerminal.Models.Requests
{
    public class DisplayTerminalOutputRequest
    {
        public int TerminalId { get; set; }
        public string Output { get; set; }
    }
}
