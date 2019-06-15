namespace FluentTerminal.Models.Requests
{
    public class GetAvailablePortRequest : IMessage
    {
        public const byte Identifier = 2;

        byte IMessage.Identifier => Identifier;
    }
}
