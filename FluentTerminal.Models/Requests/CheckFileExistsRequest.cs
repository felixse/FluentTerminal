namespace FluentTerminal.Models.Requests
{
    public class CheckFileExistsRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.CheckFileExistsRequest;

        public string Path { get; set; }
    }
}