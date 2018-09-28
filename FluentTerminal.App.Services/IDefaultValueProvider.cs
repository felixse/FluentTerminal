using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface IDefaultValueProvider
    {
        ApplicationSettings GetDefaultApplicationSettings();

        ICollection<KeyBinding> GetDefaultKeyBindings(AppCommand command);

        Guid GetDefaultShellProfileId();

        IEnumerable<TabTheme> GetDefaultTabThemes();

        TerminalOptions GetDefaultTerminalOptions();

        Guid GetDefaultThemeId();

        IEnumerable<ShellProfile> GetPreinstalledShellProfiles();

        IEnumerable<TerminalTheme> GetPreInstalledThemes();
    }
}