using System;

namespace FluentTerminal.Models.Messages
{
    public class ThemeAddedMessage
    {
        public TerminalTheme Theme { get; }

        public ThemeAddedMessage(TerminalTheme theme)
        {
            Theme = theme ?? throw new ArgumentNullException();
        }
    }
}