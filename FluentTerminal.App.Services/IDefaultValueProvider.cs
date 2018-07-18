using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface IDefaultValueProvider
    {
        IEnumerable<ShellProfile> GetPreinstalledShellProfiles();

        Guid GetDefaultShellProfileId();

        TerminalOptions GetDefaultTerminalOptions();

        ICollection<KeyBinding> GetDefaultKeyBindings(Command command);

        ApplicationSettings GetDefaultApplicationSettings();

        Guid GetDefaultThemeId();

        IEnumerable<TerminalTheme> GetPreInstalledThemes();
    }
}