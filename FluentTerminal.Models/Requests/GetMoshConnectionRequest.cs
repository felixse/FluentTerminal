using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.Models.Requests
{
    public class GetMoshConnectionRequest
    {
        public string Host { get; set; }

        public ushort SshPort { get; set; }

        public string Username { get; set; }

        public string IdentityFile { get; set; }

        public string MoshPorts { get; set; }
    }
}
