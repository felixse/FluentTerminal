namespace FluentTerminal.Models.Requests
{
    public class QuitApplicationRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.QuitApplicationRequest;
    }
}
