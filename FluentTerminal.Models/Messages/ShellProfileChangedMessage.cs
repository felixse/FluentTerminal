using System;

namespace FluentTerminal.Models.Messages
{
    public class ShellProfileChangedMessage
    {
        public ShellProfile ShellProfile { get; }

        public ShellProfileChangedMessage(ShellProfile shellProfile)
        {
            ShellProfile = shellProfile ?? throw new ArgumentNullException();
        }
    }
}