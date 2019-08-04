namespace FluentTerminal.Models.Responses
{
    public class CreateTerminalResponse : TerminalResponse, IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.CreateTerminalResponse;

        public string ShellExecutableName { get; set; }
    }
}