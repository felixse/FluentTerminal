namespace FluentTerminal.Models.Requests
{
    public class UpdateSettingsRequest : IMessage
    {
        public const byte Identifier = 15;

        byte IMessage.Identifier => Identifier;

        public ApplicationSettings Settings { get; set; }
    }
}
