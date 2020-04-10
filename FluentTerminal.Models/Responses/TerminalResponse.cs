namespace FluentTerminal.Models.Responses
{
    public abstract class TerminalResponse : IMessage
    {
        public abstract byte Identifier { get; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
