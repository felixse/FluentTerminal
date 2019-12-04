﻿using FluentTerminal.Models;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        void DeleteShellProfile(Guid id);
        void DeleteSshProfile(Guid id);
        void DeleteTheme(Guid id);
        ApplicationSettings GetApplicationSettings();
        TerminalTheme GetCurrentTheme();
        Guid GetCurrentThemeId();
        ShellProfile GetDefaultShellProfile();
        Guid GetDefaultShellProfileId();
        IDictionary<string, ICollection<KeyBinding>> GetCommandKeyBindings();
        IEnumerable<ShellProfile> GetShellProfiles();
        IEnumerable<SshProfile> GetSshProfiles();
        /// <summary>
        /// Returns union of <see cref="GetShellProfiles"/> and <see cref="GetSshProfiles"/>.
        /// </summary>
        IEnumerable<ShellProfile> GetAllProfiles();
        IEnumerable<TabTheme> GetTabThemes();
        TerminalOptions GetTerminalOptions();
        TerminalTheme GetTheme(Guid id);
        IEnumerable<TerminalTheme> GetThemes();
        void ResetKeyBindings();
        void SaveApplicationSettings(ApplicationSettings applicationSettings);
        void NotifyApplicationSettingsChanged(ApplicationSettings applicationSettings);
        void SaveCurrentThemeId(Guid id);
        void SaveDefaultShellProfileId(Guid id);
        void SaveKeyBindings(string command, ICollection<KeyBinding> keyBindings);
        void SaveShellProfile(ShellProfile shellProfile, bool newShell = false);
        void SaveSshProfile(SshProfile sshProfile, bool newShell = false);
        void SaveTerminalOptions(TerminalOptions terminalOptions);
        void SaveTheme(TerminalTheme theme, bool newTheme = false);
        ShellProfile GetShellProfile(Guid id);
        SshProfile GetSshProfile(Guid id);
        string ExportSettings();
        void ImportSettings(string serializedSettings);
    }
}