using Dahomey.Json.Attributes;
using FluentTerminal.Models;


namespace FluentTerminal.App.WebViewMessages
{
    [JsonDiscriminator("terminal-resized")]
    public class TerminalResizedMessage : WebViewMessageBase
    {
        public TerminalSize Size { get; set; }
    }
}
