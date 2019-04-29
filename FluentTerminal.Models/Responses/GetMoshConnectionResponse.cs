using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.Models.Responses
{
    public class GetMoshConnectionResponse
    {
        public string Key { get; set; }
        public string Port { get; set; }
        public string FilePath { get; set; }
    }
}
