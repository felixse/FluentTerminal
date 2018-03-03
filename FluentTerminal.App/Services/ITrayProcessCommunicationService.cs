using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellConfiguration shellConfiguration);
        Task ResizeTerminal(int id, TerminalSize size);
        Task UpdateToggleWindowKeyBindings();
    }
}
