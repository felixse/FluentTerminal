using FluentTerminal.Models;
using FluentTerminal.Models.Responses;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        Task Initialize(IAppServiceConnection appServiceConnection);
        Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellProfile shellProfile);
        Task ResizeTerminal(int id, TerminalSize size);
        Task UpdateToggleWindowKeyBindings();
    }
}
