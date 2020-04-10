namespace FluentTerminal.Models.Responses
{
    public class PauseTerminalOutputResponse : TerminalResponse
    {
        public override byte Identifier => (byte) MessageIdentifiers.PauseTerminalOutputResponse;
    }
}