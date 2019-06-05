namespace FluentTerminal.Models.Requests
{
    public class GetUserNameRequest : IMessage
    {
        public const byte Identifier = 3;

        byte IMessage.Identifier => Identifier;
    }
}
