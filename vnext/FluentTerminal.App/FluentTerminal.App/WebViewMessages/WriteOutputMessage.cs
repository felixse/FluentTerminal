using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.WebViewMessages
{
    [JsonDiscriminator("write-output")]
    public class WriteOutputMessage : WebViewMessageBase
    {
        public byte[] Data { get; set; }
    }
}
