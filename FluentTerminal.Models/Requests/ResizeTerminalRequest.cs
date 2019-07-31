namespace FluentTerminal.Models.Requests
{
    public class ResizeTerminalRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.ResizeTerminalRequest;

        public byte TerminalId { get; set; }

        public TerminalSize NewSize { get; set; }
    }
}