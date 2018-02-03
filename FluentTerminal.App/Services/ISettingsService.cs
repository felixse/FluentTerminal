using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ISettingsService
    {
        ShellConfiguration GetShellConfiguration();
        void SaveShellConfiguration(ShellConfiguration spawnConfiguration);
    }
}
