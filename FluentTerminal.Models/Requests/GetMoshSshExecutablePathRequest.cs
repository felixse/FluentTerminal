namespace FluentTerminal.Models.Requests
{
    public class GetMoshSshExecutablePathRequest : IMessage
    {
        public const byte Identifier = 8;

        byte IMessage.Identifier => Identifier;

        public bool IsMosh { get; set; }
    }
}