namespace FluentTerminal.Models.Requests
{
    public class PauseTerminalOutputRequest : IMessage
    {
        public const byte Identifier = 16;

        byte IMessage.Identifier => Identifier;

        public bool Pause { get; set; }

        public byte Id { get; set; }
    }
}