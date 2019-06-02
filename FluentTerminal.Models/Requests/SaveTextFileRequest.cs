namespace FluentTerminal.Models.Requests
{
    public class SaveTextFileRequest : IMessage
    {
        public const byte Identifier = 5;

        byte IMessage.Identifier => Identifier;

        public string Path { get; set; }

        public string Content { get; set; }
    }
}