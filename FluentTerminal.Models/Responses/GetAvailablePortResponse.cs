namespace FluentTerminal.Models.Responses
{
    public class GetAvailablePortResponse : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.GetAvailablePortResponse;

        public int Port { get; set; }
    }
}
