namespace FluentTerminal.Models.Requests
{
    public class MuteTerminalRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.MuteTerminalRequest;

        public bool Mute { get; set; }
    }
}
