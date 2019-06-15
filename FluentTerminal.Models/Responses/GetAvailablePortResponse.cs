namespace FluentTerminal.Models.Responses
{
    public class GetAvailablePortResponse : IMessage
    {
        public const byte Identifier = 10;

        byte IMessage.Identifier => Identifier;

        public int Port { get; set; }
    }
}
