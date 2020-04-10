namespace FluentTerminal.Models.Responses
{
    public class CreateTerminalResponse : TerminalResponse
    {
        public override byte Identifier => (byte) MessageIdentifiers.CreateTerminalResponse;

        public string Name { get; set; }
    }
}