namespace FluentTerminal.Models.Requests
{
    public class ResizeTerminalRequest : IMessage
    {
        public const byte Identifier = 4;

        byte IMessage.Identifier => Identifier;

        public byte TerminalId { get; set; }

        public TerminalSize NewSize { get; set; }
    }
}