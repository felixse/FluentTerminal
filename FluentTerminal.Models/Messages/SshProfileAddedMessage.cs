using System;

namespace FluentTerminal.Models.Messages
{
    public class SshProfileAddedMessage
    {
        public SshProfile SshProfile { get; }

        public SshProfileAddedMessage(SshProfile sshProfile)
        {
            SshProfile = sshProfile ?? throw new ArgumentNullException();
        }
    }
}