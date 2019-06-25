namespace FluentTerminal.Models.Responses
{
    public class GetSshConfigFolderResponse : IMessage
    {
        public const byte Identifier = 13;

        byte IMessage.Identifier => Identifier;

        public bool Success { get; set; }

        public string Error { get; set; }

        public string Path { get; set; }

        public string[] Files { get; set; }
    }
}