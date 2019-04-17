using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Responses;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        event EventHandler<int> TerminalExited;

        void Initialize(IAppServiceConnection appServiceConnection);

        Task<CreateTerminalResponse> CreateTerminal(int id, TerminalSize size, ShellProfile shellProfile,
            SessionType sessionType);

        Task ResizeTerminal(int id, TerminalSize size);

        Task UpdateToggleWindowKeyBindings();

        Task Write(int terminalId, byte[] data);

        void SubscribeForTerminalOutput(int terminalId, Action<byte[]> callback);

        Task CloseTerminal(int terminalId);
        Task<GetAvailablePortResponse> GetAvailablePort();
        int GetNextTerminalId();
    }
}