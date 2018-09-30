﻿using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        event EventHandler<ApplicationSettings> ApplicationSettingsChanged;
        event EventHandler<Guid> CurrentThemeChanged;
        event EventHandler KeyBindingsChanged;
        event EventHandler<Tuple<bool, ShellProfile>> ShellProfileCollectionChanged;
        event EventHandler<TerminalOptions> TerminalOptionsChanged;
        void DeleteShellProfile(Guid id);
        void DeleteTheme(Guid id);
        ApplicationSettings GetApplicationSettings();
        TerminalTheme GetCurrentTheme();
        Guid GetCurrentThemeId();
        ShellProfile GetDefaultShellProfile();
        Guid GetDefaultShellProfileId();
        IDictionary<AbstractCommand, ICollection<KeyBinding>> GetKeyBindings();
        IEnumerable<ShellProfile> GetShellProfiles();
        IEnumerable<TabTheme> GetTabThemes();
        TerminalOptions GetTerminalOptions();
        TerminalTheme GetTheme(Guid id);
        IEnumerable<TerminalTheme> GetThemes();
        void ResetKeyBindings();
        void SaveApplicationSettings(ApplicationSettings applicationSettings);
        void SaveCurrentThemeId(Guid id);
        void SaveDefaultShellProfileId(Guid id);
        void SaveKeyBindings(AbstractCommand command, ICollection<KeyBinding> keyBindings);
        void SaveShellProfile(ShellProfile shellProfile, bool newShell = false);
        void SaveTerminalOptions(TerminalOptions terminalOptions);
        void SaveTheme(TerminalTheme theme);
        AbstractCommand ParseCommandString(string command);
    }
}