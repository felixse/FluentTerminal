using System;

namespace FluentTerminal.Models.Messages
{
    public class ShellProfileDeletedMessage
    {
        public Guid ProfileId { get; }

        public ShellProfileDeletedMessage(Guid profileId)
        {
            ProfileId = profileId;
        }
    }
}