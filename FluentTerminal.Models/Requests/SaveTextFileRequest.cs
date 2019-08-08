namespace FluentTerminal.Models.Requests
{
    public class SaveTextFileRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.SaveTextFileRequest;

        public string Path { get; set; }

        public string Content { get; set; }
    }
}