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
        event EventHandler<SshShellProfile> SshShellProfileAdded;
        event EventHandler<Guid> SshShellProfileDeleted;
        event EventHandler<TerminalOptions> TerminalOptionsChanged;
        event EventHandler<Guid> ThemeDeleted;
        event EventHandler<TerminalTheme> ThemeAdded;

        void DeleteShellProfile(Guid id);
        void DeleteSshShellProfile(Guid id);
        void DeleteTheme(Guid id);
        ApplicationSettings GetApplicationSettings();
        TerminalTheme GetCurrentTheme();
        Guid GetCurrentThemeId();
        ShellProfile GetDefaultShellProfile();
        SshShellProfile GetDefaultSshShellProfile();
        Guid GetDefaultShellProfileId();
        Guid GetDefaultSshShellProfileId();
        IDictionary<string, ICollection<KeyBinding>> GetCommandKeyBindings();
        IEnumerable<ShellProfile> GetShellProfiles();
        IEnumerable<SshShellProfile> GetSshShellProfiles();
        IEnumerable<TabTheme> GetTabThemes();
        TerminalOptions GetTerminalOptions();
        TerminalTheme GetTheme(Guid id);
        IEnumerable<TerminalTheme> GetThemes();
        void ResetKeyBindings();
        void SaveApplicationSettings(ApplicationSettings applicationSettings);
        void SaveCurrentThemeId(Guid id);
        void SaveDefaultShellProfileId(Guid id);
        void SaveDefaultSshShellProfileId(Guid id);
        void SaveKeyBindings(string command, ICollection<KeyBinding> keyBindings);
        void SaveShellProfile(ShellProfile shellProfile, bool newShell = false);
        void SaveSshShellProfile(SshShellProfile shellProfile, bool newShell = false);
        void SaveTerminalOptions(TerminalOptions terminalOptions);
        void SaveTheme(TerminalTheme theme, bool newTheme = false);
        ShellProfile GetShellProfile(Guid id);
        SshShellProfile GetSshShellProfile(Guid id);
        string ExportSettings();
        void ImportSettings(string serializedSettings);
    }
}