using System;

namespace FluentTerminal.Models.Messages
{
    public class SshProfileDeletedMessage
    {
        public Guid ProfileId { get; }

        public SshProfileDeletedMessage(Guid profileId)
        {
            ProfileId = profileId;
        }
    }
}