using FluentTerminal.Models;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        event EventHandler CurrentThemeChanged;

        ShellConfiguration GetShellConfiguration();
        void SaveShellConfiguration(ShellConfiguration spawnConfiguration);

        TerminalColors GetCurrentThemeColors();
        Guid GetCurrentThemeId();
        TerminalColors GetThemeColors(Guid id);
        void SaveCurrentThemeId(Guid id);

        IEnumerable<TerminalTheme> GetThemes();
        void SaveTheme(TerminalTheme theme);
        void DeleteTheme(Guid id);
    }
}
