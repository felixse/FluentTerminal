using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentTerminal.Models
{
    public class SshProfile: ShellProfile
    {
        private string _sshLocation;

        public SshProfile()
        {
            WorkingDirectory = string.Empty;

            _sshLocation = getSshLocation();
        }

        private string getSshLocation()
        {
            //
            // See https://stackoverflow.com/a/25919981
            //

            string system32Folder;

            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                system32Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Sysnative");
            }
            else
            {
                system32Folder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            }

            return Path.Combine(system32Folder, @"OpenSSH\ssh.exe");
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
                return _sshLocation;
            }
        }

        public override string ValidateAndGetErrors()
        {
            string result = null;

            if( !System.IO.File.Exists(_sshLocation) )
            {
                result = "The OpenSSH client seems not installed on this machine";
            }

            return result;
        }

        public ushort Port { get; set; }
        public string Username { get; set; }
        public string Host { get; set; }
    }
}
