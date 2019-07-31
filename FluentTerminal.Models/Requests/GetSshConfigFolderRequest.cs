namespace FluentTerminal.Models.Requests
{
    public class GetSshConfigFolderRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.GetSshConfigFolderRequest;

        public bool IncludeContent { get; set; }
    }
}