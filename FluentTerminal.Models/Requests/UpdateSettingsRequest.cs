namespace FluentTerminal.Models.Requests
{
    public class UpdateSettingsRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.UpdateSettingsRequest;

        public ApplicationSettings Settings { get; set; }
    }
}
