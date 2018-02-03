using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface IDefaultValueProvider
    {
        ShellConfiguration GetDefaultShellConfiguration();
    }
}
