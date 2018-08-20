using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        event EventHandler<Guid> CurrentThemeChanged;

        event EventHandler<TerminalOptions> TerminalOptionsChanged;

        event EventHandler<ApplicationSettings> ApplicationSettingsChanged;

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

        IDictionary<Command, ICollection<KeyBinding>> GetKeyBindings();

        void SaveKeyBindings(Command command, ICollection<KeyBinding> keyBindings);

        void ResetKeyBindings();

        TerminalTheme GetCurrentTheme();

        Guid GetCurrentThemeId();

        TerminalTheme GetTheme(Guid id);

        void SaveCurrentThemeId(Guid id);

        IEnumerable<TerminalTheme> GetThemes();

        void SaveTheme(TerminalTheme theme);

        void DeleteTheme(Guid id);
    }
}