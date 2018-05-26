using FluentTerminal.Models;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        event EventHandler CurrentThemeChanged;
        event EventHandler TerminalOptionsChanged;
        event EventHandler ApplicationSettingsChanged;
        event EventHandler KeyBindingsChanged;

        Guid GetDefaultShellProfileId();
        ShellProfile GetDefaultShellProfile();
        void SaveDefaultShellProfileId(Guid id);

        IEnumerable<ShellProfile> GetShellProfiles();
        void SaveShellProfile(ShellProfile shellProfile);
        void DeleteShellProfile(Guid id);

        TerminalOptions GetTerminalOptions();
        void SaveTerminalOptions(TerminalOptions terminalOptions);

        ApplicationSettings GetApplicationSettings();
        void SaveApplicationSettings(ApplicationSettings applicationSettings);

        KeyBindings GetKeyBindings();
        void SaveKeyBindings(KeyBindings keyBindings);

        TerminalTheme GetCurrentTheme();
        Guid GetCurrentThemeId();
        TerminalTheme GetTheme(Guid id);
        void SaveCurrentThemeId(Guid id);

        IEnumerable<TerminalTheme> GetThemes();
        void SaveTheme(TerminalTheme theme);
        void DeleteTheme(Guid id);
    }
}
