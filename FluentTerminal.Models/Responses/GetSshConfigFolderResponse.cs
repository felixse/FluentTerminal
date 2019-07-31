namespace FluentTerminal.Models.Responses
{
    public class GetSshConfigFolderResponse : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.GetSshConfigFolderResponse;

        public bool Success { get; set; }

        public string Error { get; set; }

        public string Path { get; set; }

        public string[] Files { get; set; }
    }
}