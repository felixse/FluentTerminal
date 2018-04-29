using FluentTerminal.Models;
using FluentTerminal.Models.Responses;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        Task Initialize(AppServiceConnection appServiceConnection);
        Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellConfiguration shellConfiguration);
        Task ResizeTerminal(int id, TerminalSize size);
        Task UpdateToggleWindowKeyBindings();
    }
}
