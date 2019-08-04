namespace FluentTerminal.Models.Responses
{
    public class PauseTerminalOutputResponse : TerminalResponse, IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.PauseTerminalOutputResponse;
    }
}