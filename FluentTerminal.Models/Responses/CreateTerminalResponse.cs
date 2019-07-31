namespace FluentTerminal.Models.Responses
{
    public class CreateTerminalResponse :IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.CreateTerminalResponse;

        public bool Success { get; set; }
        public string Error { get; set; }
        public string ShellExecutableName { get; set; }
    }
}