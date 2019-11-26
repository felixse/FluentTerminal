using System;

namespace FluentTerminal.Models.Messages
{
    public class DefaultShellProfileChangedMessage
    {
        public Guid NewDefaultProfileId { get; }

        public DefaultShellProfileChangedMessage(Guid newDefaultProfileId) => NewDefaultProfileId = newDefaultProfileId;
    }
}