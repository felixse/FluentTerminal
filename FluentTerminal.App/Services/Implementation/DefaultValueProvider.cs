using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services.Implementation
{
    internal class DefaultValueProvider : IDefaultValueProvider
    {
        public ShellConfiguration GetDefaultShellConfiguration()
        {
            return new ShellConfiguration
            {
                Shell = ShellType.PowerShell,
                CustomShellLocation = string.Empty,
                WorkingDirectory = string.Empty,
                Arguments = string.Empty
            };
        }
    }
}
