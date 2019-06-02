namespace FluentTerminal.Models.Responses
{
    public class CommonResponse : IMessage
    {
        public const byte Identifier = 8;

        byte IMessage.Identifier => Identifier;

        public bool Success { get; set; }

        public string Error { get; set; }
    }
}