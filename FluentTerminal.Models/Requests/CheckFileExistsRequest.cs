namespace FluentTerminal.Models.Requests
{
    public class CheckFileExistsRequest : IMessage
    {
        public const byte Identifier = 12;

        byte IMessage.Identifier => Identifier;

        public string Path { get; set; }
    }
}