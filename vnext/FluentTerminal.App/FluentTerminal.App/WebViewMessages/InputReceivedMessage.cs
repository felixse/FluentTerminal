using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.WebViewMessages
{
    [JsonDiscriminator("input-received")]
    internal class InputReceivedMessage : WebViewMessageBase
    {
        public string Data { get; set; }
    }
}
