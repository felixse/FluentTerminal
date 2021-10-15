using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    internal class FakeTrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        public event EventHandler<TerminalExitStatus> TerminalExited;

        public Task<bool> CheckFileExistsAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task CloseTerminalAsync(byte terminalId)
        {
            throw new NotImplementedException();
        }

        public Task<CreateTerminalResponse> CreateTerminalAsync(byte id, TerminalSize size, ShellProfile shellProfile, SessionType sessionType)
        {
            

            throw new NotImplementedException();
        }

        public Task<string> GetCommandPathAsync(string command)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetFilesFromSshConfigDirAsync()
        {
            throw new NotImplementedException();
        }

        public byte GetNextTerminalId()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSshConfigDirAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync()
        {
            throw new NotImplementedException();
        }

        public void Initialize(IAppServiceConnection appServiceConnection)
        {
            throw new NotImplementedException();
        }

        public Task MuteTerminalAsync(bool mute)
        {
            throw new NotImplementedException();
        }

        public Task<PauseTerminalOutputResponse> PauseTerminalOutputAsync(byte id, bool pause)
        {
            throw new NotImplementedException();
        }

        public Task QuitApplicationAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadTextFileAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task ResizeTerminalAsync(byte id, TerminalSize size)
        {
            throw new NotImplementedException();
        }

        public Task SaveTextFileAsync(string path, string content)
        {
            throw new NotImplementedException();
        }

        public void SubscribeForTerminalOutput(byte terminalId, Action<byte[]> callback)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeFromTerminalOutput(byte terminalId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSettings(ApplicationSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task UpdateToggleWindowKeyBindingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(byte terminalId, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
