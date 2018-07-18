using FluentTerminal.Models;
using FluentTerminal.Models.Responses;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        event EventHandler<int> TerminalExited;

        Task Initialize(IAppServiceConnection appServiceConnection);
        Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellProfile shellProfile);
        Task ResizeTerminal(int id, TerminalSize size);
        Task UpdateToggleWindowKeyBindings();
        Task WriteText(int terminalId, string text);
        void SubscribeForTerminalOutput(int terminalId, Action<string> callback);
        Task CloseTerminal(int terminalId);
    }
}
