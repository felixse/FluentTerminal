namespace FluentTerminal.Models.Requests
{
    public class GetSshConfigFolderRequest : IMessage
    {
        public const byte Identifier = 13;

        byte IMessage.Identifier => Identifier;

        public bool IncludeContent { get; set; }
    }
}