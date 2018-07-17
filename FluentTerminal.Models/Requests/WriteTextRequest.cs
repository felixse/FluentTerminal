namespace FluentTerminal.Models.Requests
{
    public class WriteTextRequest
    {
        public int TerminalId { get; set; }

        public string Text { get; set; }
    }
}
