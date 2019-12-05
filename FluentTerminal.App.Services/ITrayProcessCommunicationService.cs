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

        Task<CreateTerminalResponse> CreateTerminalAsync(byte id, TerminalSize size, ShellProfile shellProfile,
            SessionType sessionType);

        Task<PauseTerminalOutputResponse> PauseTerminalOutputAsync(byte id, bool pause);

        Task ResizeTerminalAsync(byte id, TerminalSize size);

        Task UpdateToggleWindowKeyBindingsAsync();

        Task WriteAsync(byte terminalId, byte[] data);

        void SubscribeForTerminalOutput(byte terminalId, Action<byte[]> callback);

        void UnsubscribeFromTerminalOutput(byte terminalId);

        Task CloseTerminalAsync(byte terminalId);

        byte GetNextTerminalId();

        Task<string> GetUserNameAsync();

        Task SaveTextFileAsync(string path, string content);

        Task<string> GetSshConfigDirAsync();

        Task<string[]> GetFilesFromSshConfigDirAsync();

        Task<bool> CheckFileExistsAsync(string path);

        Task MuteTerminalAsync(bool mute);

        void UpdateSettings(ApplicationSettings settings);

        Task<string> GetCommandPathAsync(string command);

        Task QuitApplicationAsync();
    }
}