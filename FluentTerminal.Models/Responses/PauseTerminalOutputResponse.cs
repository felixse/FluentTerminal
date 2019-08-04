namespace FluentTerminal.Models.Responses
{
    public class PauseTerminalOutputResponse : TerminalResponse, IMessage
    {
        public const byte Identifier = 16;

        byte IMessage.Identifier => Identifier;
    }
}