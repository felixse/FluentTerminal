namespace FluentTerminal.Models.Responses
{
    public class CreateTerminalResponse : TerminalResponse, IMessage
    {
        public const byte Identifier = 9;

        byte IMessage.Identifier => Identifier;

        public string ShellExecutableName { get; set; }
    }
}