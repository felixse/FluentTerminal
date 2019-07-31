namespace FluentTerminal.Models.Requests
{
    public class GetCommandPathRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.GetCommandPathRequest;

        public string Command { get; set; }
    }
}