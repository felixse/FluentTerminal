using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Responses;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITrayProcessCommunicationService
    {
        event EventHandler<TerminalExitStatus> TerminalExited;

        void Initialize(IAppServiceConnection appServiceConnection);

        Task<CreateTerminalResponse> CreateTerminal(byte id, TerminalSize size, ShellProfile shellProfile, SessionType sessionType);

        Task<PauseTerminalOutputResponse> PauseTerminalOutput(byte id, bool pause);

        Task ResizeTerminal(byte id, TerminalSize size);

        Task UpdateToggleWindowKeyBindings();

        Task Write(byte terminalId, byte[] data);

        void SubscribeForTerminalOutput(byte terminalId, Action<byte[]> callback);

        Task CloseTerminal(byte terminalId);
        Task<GetAvailablePortResponse> GetAvailablePort();
        byte GetNextTerminalId();

        Task<string> GetUserName();

        Task SaveTextFileAsync(string path, string content);

        Task<string> GetSshConfigDirAsync();

        Task<string[]> GetFilesFromSshConfigDirAsync();

        Task<bool> CheckFileExistsAsync(string path);

        void MuteTerminal(bool mute);

        void UpdateSettings(ApplicationSettings settings);
    }
}