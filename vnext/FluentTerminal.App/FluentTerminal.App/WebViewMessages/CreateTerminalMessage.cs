using Dahomey.Json.Attributes;
using FluentTerminal.Models;
using System.Collections.Generic;

namespace FluentTerminal.App.WebViewMessages
{
    [JsonDiscriminator("create-terminal")]
    public class CreateTerminalMessage : WebViewMessageBase
    {
        public TerminalOptions Options { get; set; }

        public TerminalColors Colors { get; set; }

        public IEnumerable<KeyBinding> KeyBindings { get; set; }
    }
}
