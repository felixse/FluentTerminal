using System;

namespace FluentTerminal.Models.Messages
{
    public class ThemeDeletedMessage
    {
        public Guid ThemeId { get; }

        public ThemeDeletedMessage(Guid themeId)
        {
            ThemeId = themeId;
        }
    }
}