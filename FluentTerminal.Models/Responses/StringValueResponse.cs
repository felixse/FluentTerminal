namespace FluentTerminal.Models.Responses
{
    public class StringValueResponse : CommonResponse
    {
        public override byte Identifier => (byte) MessageIdentifiers.StringValueResponse;

        public string Value { get; set; }
    }
}