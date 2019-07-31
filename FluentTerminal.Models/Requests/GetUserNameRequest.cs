namespace FluentTerminal.Models.Requests
{
    public class GetUserNameRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.GetUserNameRequest;
    }
}
