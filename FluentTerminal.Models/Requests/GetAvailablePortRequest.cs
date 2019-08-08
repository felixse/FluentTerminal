namespace FluentTerminal.Models.Requests
{
    public class GetAvailablePortRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.GetAvailablePortRequest;
    }
}
