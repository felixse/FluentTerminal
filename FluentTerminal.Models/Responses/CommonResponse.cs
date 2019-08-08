namespace FluentTerminal.Models.Responses
{
    public class CommonResponse : IMessage
    {
        public virtual byte Identifier => (byte) MessageIdentifiers.CommonResponse;

        public bool Success { get; set; }

        public string Error { get; set; }
    }
}