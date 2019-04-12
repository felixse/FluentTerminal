using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentTerminal.Models
{
    public class SshProfile: ShellProfile
    {
        public SshProfile()
        {
            WorkingDirectory = string.Empty;
        }

        public override string Arguments
        {
            get
            {
                return $"-p {Port:#####} {Username}@{Host}";
            }
        }

        public override string Location
        {
            get
            {
                //
                // See https://stackoverflow.com/a/25919981
                //

                string system32Folder;

                if(Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                {
                    system32Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Sysnative");
                }
                else
                {
                    system32Folder = Environment.GetFolderPath(Environment.SpecialFolder.System);
                }

                return Path.Combine(system32Folder, @"OpenSSH\ssh.exe");
            }
        }

        public ushort Port { get; set; }
        public string Username { get; set; }
        public string Host { get; set; }
    }
}
