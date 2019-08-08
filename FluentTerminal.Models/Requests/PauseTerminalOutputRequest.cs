namespace FluentTerminal.Models.Requests
{
    public class PauseTerminalOutputRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.PauseTerminalOutputRequest;

        public bool Pause { get; set; }

        public byte Id { get; set; }
    }
}