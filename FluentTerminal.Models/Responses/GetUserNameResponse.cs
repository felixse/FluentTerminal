namespace FluentTerminal.Models.Responses
{
    public class GetUserNameResponse : IMessage
    {
        public const byte Identifier = 11;

        byte IMessage.Identifier => Identifier;

        public string UserName { get; set; }
    }
}
