using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        Task Initialize(int port);
        Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellConfiguration shellConfiguration);
        Task ResizeTerminal(int id, TerminalSize size);
        Task UpdateToggleWindowKeyBindings();
    }
}
