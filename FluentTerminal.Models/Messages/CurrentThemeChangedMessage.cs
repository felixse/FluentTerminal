using System;

namespace FluentTerminal.Models.Messages
{
    public class CurrentThemeChangedMessage
    {
        public Guid ThemeId { get; }

        public CurrentThemeChangedMessage(Guid themeId)
        {
            ThemeId = themeId;
        }
    }
}