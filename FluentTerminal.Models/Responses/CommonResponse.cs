namespace FluentTerminal.Models.Responses
{
    public class CommonResponse : TerminalResponse
    {
        public override byte Identifier => (byte) MessageIdentifiers.CommonResponse;
    }
}