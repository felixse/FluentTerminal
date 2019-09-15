using System;

namespace FluentTerminal.Models.Messages
{
    public class ShellProfileAddedMessage
    {
        public ShellProfile ShellProfile { get; }

        public ShellProfileAddedMessage(ShellProfile shellProfile)
        {
            ShellProfile = shellProfile ?? throw new ArgumentNullException();
        }
    }
}