namespace FluentTerminal.Models.Requests
{
    public class ReadTextFileRequest : IMessage
    {
        public byte Identifier => (byte)MessageIdentifiers.ReadTextFileRequest;

        public string Path { get; set; }
    }
}
