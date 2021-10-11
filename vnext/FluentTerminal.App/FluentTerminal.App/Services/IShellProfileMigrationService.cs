using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface IShellProfileMigrationService
    {
        void Migrate(ShellProfile profile);
    }
}
