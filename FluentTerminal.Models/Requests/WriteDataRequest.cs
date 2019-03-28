namespace FluentTerminal.Models.Requests
{
    public class WriteDataRequest
    {
        public int TerminalId { get; set; }

        public byte[] Data { get; set; }
    }
}