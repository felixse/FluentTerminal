using Dahomey.Json.Attributes;

namespace FluentTerminal.App.WebViewMessages
{
    [JsonDiscriminator("title-changed")]
    public class TitleChangedMessage : WebViewMessageBase
    {
        public string Title { get; set; }
    }
}
