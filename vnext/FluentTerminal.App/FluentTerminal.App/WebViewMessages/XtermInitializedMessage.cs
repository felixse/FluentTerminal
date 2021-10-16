using Dahomey.Json.Attributes;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.WebViewMessages
{
    [JsonDiscriminator("xterm-initialized")]
    public class XtermInitializedMessage : WebViewMessageBase
    {
        public TerminalSize Size { get; set; }
    }
}
