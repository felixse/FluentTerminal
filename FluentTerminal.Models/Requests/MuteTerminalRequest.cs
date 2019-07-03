namespace FluentTerminal.Models.Requests
{
    public class MuteTerminalRequest : IMessage
    {
        public const byte Identifier = 14;

        byte IMessage.Identifier => Identifier;

        public bool Mute { get; set; }
    }
}
