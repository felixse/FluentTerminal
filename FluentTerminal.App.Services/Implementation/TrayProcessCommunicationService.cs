using FluentTerminal.App.Services.Exceptions;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace FluentTerminal.App.Services.Implementation
{
    public class TrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        #region Static

        private static string _userName;
        private static string _sshConfigDir;
        private static readonly Dictionary<string, string> CommandPaths = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> CommandErrors = new Dictionary<string, string>();

        private static ValueSet CreateMessage(IMessage content)
        {
            return new ValueSet
            {
                [MessageKeys.Type] = content.Identifier,
                [MessageKeys.Content] = JsonConvert.SerializeObject(content)
            };
        }

        #endregion Static

        private readonly ISettingsService _settingsService;
        private IAppServiceConnection _appServiceConnection;
        // ReSharper disable once RedundantDefaultMemberInitializer
        private byte _nextTerminalId = 0;

        public event EventHandler<TerminalExitStatus> TerminalExited;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public byte GetNextTerminalId()
        {
            return _nextTerminalId++;
        }

        public async Task<string> GetUserNameAsync()
        {
            if (!string.IsNullOrEmpty(_userName))
            {
                // Returning the username from cache
                return _userName;
            }

            var response = await GetResponseAsync<StringValueResponse>(new GetUserNameRequest()).ConfigureAwait(false);

            if (response.Success)
            {
                _userName = response.Value;
            }

            return _userName;
        }

        public async Task SaveTextFileAsync(string path, string content)
        {
            var response =
                await GetResponseAsync<CommonResponse>(new SaveTextFileRequest {Path = path, Content = content})
                    .ConfigureAwait(false);

            if (!response.Success)
            {
                throw new SaveTextFileException(string.IsNullOrEmpty(response.Error)
                    ? "Failed to save the file."
                    : response.Error);
            }
        }

        public async Task<string> GetSshConfigDirAsync()
        {
            if (!string.IsNullOrEmpty(_sshConfigDir))
            {
                return _sshConfigDir;
            }

            var response =
                await GetResponseAsync<GetSshConfigFolderResponse>(new GetSshConfigFolderRequest
                    { IncludeContent = false }).ConfigureAwait(false);

            if (response.Success)
            {
                _sshConfigDir = response.Path;
            }

            return _sshConfigDir;
        }

        public async Task<string[]> GetFilesFromSshConfigDirAsync()
        {
            var response =
                await GetResponseAsync<GetSshConfigFolderResponse>(new GetSshConfigFolderRequest
                    {IncludeContent = true}).ConfigureAwait(false);

            if (response.Success)
            {
                _sshConfigDir = response.Path;

                return response.Files;
            }

            return null;
        }

        public async Task<bool> CheckFileExistsAsync(string path)
        {
            var response = await GetResponseAsync<CommonResponse>(new CheckFileExistsRequest {Path = path})
                .ConfigureAwait(false);

            return response.Success;
        }

        public Task MuteTerminalAsync(bool mute)
        {
            return _appServiceConnection.SendMessageAsync(CreateMessage(new MuteTerminalRequest {Mute = mute}));
        }

        public void UpdateSettings(ApplicationSettings settings)
        {
            _appServiceConnection.SendMessageAsync(CreateMessage(new UpdateSettingsRequest { Settings = settings }));
        }

        public async Task<CreateTerminalResponse> CreateTerminalAsync(byte id, TerminalSize size, ShellProfile shellProfile,
            SessionType sessionType)
        {
            var request = new CreateTerminalRequest
            {
                Id = id,
                Size = size,
                Profile = shellProfile,
                SessionType = sessionType
            };

            Logger.Instance.Debug("Sending CreateTerminalRequest: {@request}", request);

            var response = await GetResponseAsync<CreateTerminalResponse>(request).ConfigureAwait(false);

            Logger.Instance.Debug("Received CreateTerminalResponse: {@response}", response);

            return response;
        }

        public Task<PauseTerminalOutputResponse> PauseTerminalOutputAsync(byte id, bool pause)
        {
            return GetResponseAsync<PauseTerminalOutputResponse>(
                new PauseTerminalOutputRequest {Id = id, Pause = pause});
        }

        public void Initialize(IAppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, IDictionary<string, object> e)
        {
            var messageType = (byte)e[MessageKeys.Type];
            var messageContent = e[MessageKeys.Content];

            switch (messageType)
            {
                case (byte) MessageIdentifiers.TerminalExitedRequest:
                    var request = JsonConvert.DeserializeObject<TerminalExitedRequest>((string)messageContent);
                    Logger.Instance.Debug("Received TerminalExitedRequest: {@request}", request);

                    TerminalExited?.Invoke(this, request.ToStatus());
                    break;
                default:
                    Logger.Instance.Error("Received unknown message type: {messageType}", messageType);
                    break;
            }
        }

        public Task ResizeTerminalAsync(byte id, TerminalSize size)
        {
            return _appServiceConnection.SendMessageAsync(CreateMessage(new ResizeTerminalRequest
                {TerminalId = id, NewSize = size}));
        }

        public Task UpdateToggleWindowKeyBindingsAsync()
        {
            var keyBindings = _settingsService.GetCommandKeyBindings()[nameof(Command.ToggleWindow)];

            return _appServiceConnection.SendMessageAsync(CreateMessage(new SetToggleWindowKeyBindingsRequest
                {KeyBindings = keyBindings}));
        }

        public Task WriteAsync(byte id, byte[] data)
        {
            var message = new ValueSet
            {
                [MessageKeys.Type] = Constants.TerminalBufferRequestIdentifier,
                [MessageKeys.TerminalId] = id,
                [MessageKeys.Content] = data
            };

            return _appServiceConnection.SendMessageAsync(message);
        }

        public Task CloseTerminalAsync(byte terminalId)
        {
            var request = new TerminalExitedRequest(terminalId, -1);

            Logger.Instance.Debug("Sending TerminalExitedRequest: {@request}", request);

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        public async Task<string> GetCommandPathAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Input value is null or empty.", nameof(command));
            }

            command = command.Trim();

            var commandLower = command.ToLowerInvariant();

            if (CommandPaths.TryGetValue(commandLower, out var path))
            {
                return path;
            }

            if (CommandErrors.TryGetValue(commandLower, out var error))
            {
                throw new Exception(error);
            }

            var response = await GetResponseAsync<StringValueResponse>(new GetCommandPathRequest {Command = command})
                .ConfigureAwait(false);

            if (response.Success)
            {
                CommandPaths[commandLower] = response.Value;

                return response.Value;
            }

            CommandErrors[commandLower] = response.Error;

            throw new Exception(response.Error);
        }

        public Task QuitApplicationAsync()
        {
            return _appServiceConnection.SendMessageAsync(CreateMessage(new QuitApplicationRequest()));
        }

        private async Task<T> GetResponseAsync<T>(IMessage request)
        {
            var messageResponse =
                await _appServiceConnection.SendMessageAsync(CreateMessage(request)).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>((string)messageResponse[MessageKeys.Content]);
        }
    }
}
