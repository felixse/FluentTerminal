namespace FluentTerminal.Models.Responses
{
    public class CreateTerminalResponse :IMessage
    {
        public const byte Identifier = 9;

        byte IMessage.Identifier => Identifier;

        public bool Success { get; set; }
        public string Error { get; set; }
        public string ShellExecutableName { get; set; }
    }
}