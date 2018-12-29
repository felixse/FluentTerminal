using FluentTerminal.Models;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        event EventHandler<ApplicationSettings> ApplicationSettingsChanged;
        event EventHandler<Guid> CurrentThemeChanged;
        event EventHandler KeyBindingsChanged;
        event EventHandler<ShellProfile> ShellProfileAdded;
        event EventHandler<Guid> ShellProfileDeleted;
        event EventHandler<TerminalOptions> TerminalOptionsChanged;
        void DeleteShellProfile(Guid id);
        void DeleteTheme(Guid id);
        ApplicationSettings GetApplicationSettings();
        TerminalTheme GetCurrentTheme();
        Guid GetCurrentThemeId();
        ShellProfile GetDefaultShellProfile();
        Guid GetDefaultShellProfileId();
        IDictionary<string, ICollection<KeyBinding>> GetCommandKeyBindings();
        IEnumerable<ShellProfile> GetShellProfiles();
        IEnumerable<TabTheme> GetTabThemes();
        TerminalOptions GetTerminalOptions();
        TerminalTheme GetTheme(Guid id);
        IEnumerable<TerminalTheme> GetThemes();
        void ResetKeyBindings();
        void SaveApplicationSettings(ApplicationSettings applicationSettings);
        void SaveCurrentThemeId(Guid id);
        void SaveDefaultShellProfileId(Guid id);
        void SaveKeyBindings(string command, ICollection<KeyBinding> keyBindings);
        void SaveShellProfile(ShellProfile shellProfile, bool newShell = false);
        void SaveTerminalOptions(TerminalOptions terminalOptions);
        void SaveTheme(TerminalTheme theme);
    }
}