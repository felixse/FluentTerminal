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
        private readonly ISettingsService _settingsService;
        private IAppServiceConnection _appServiceConnection;
        private readonly Dictionary<byte, Action<byte[]>> _terminalOutputHandlers;
        private byte _nextTerminalId = 0;

        public event EventHandler<TerminalExitStatus> TerminalExited;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _terminalOutputHandlers = new Dictionary<byte, Action<byte[]>>();
        }

        public byte GetNextTerminalId()
        {
            return _nextTerminalId++;
        }

        public async Task<GetAvailablePortResponse> GetAvailablePort()
        {
            var request = new GetAvailablePortRequest();

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(request));
            var response = JsonConvert.DeserializeObject<GetAvailablePortResponse>((string)responseMessage[MessageKeys.Content]);

            Logger.Instance.Debug("Received GetAvailablePortResponse: {@response}", response);

            return response;
        }

        private string _userName;

        public async Task<string> GetUserName()
        {
            if (!string.IsNullOrEmpty(_userName))
            {
                // Returning the username from cache
                return _userName;
            }

            StringValueResponse response;

            // No need to crash for username, so try/catch
            try
            {
                var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(new GetUserNameRequest()));
                response = JsonConvert.DeserializeObject<StringValueResponse>((string)responseMessage[MessageKeys.Content]);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e, "Error while trying to get username.");

                return null;
            }

            Logger.Instance.Debug("Received GetUserNameResponse: {@response}", response);

            _userName = response.Value;

            return _userName;
        }

        public async Task SaveTextFileAsync(string path, string content)
        {
            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(new SaveTextFileRequest {Path = path, Content = content}));

            var response = JsonConvert.DeserializeObject<CommonResponse>((string)responseMessage[MessageKeys.Content]);

            if (!response.Success)
            {
                throw new Exception(string.IsNullOrEmpty(response.Error) ? "Failed to save the file." : response.Error);
            }
        }

        private static string _sshConfigDir;

        public async Task<string> GetSshConfigDirAsync()
        {
            if (string.IsNullOrEmpty(_sshConfigDir))
            {
                var response = await GetSshConfigFolderAsync(false);

                if (response?.Success ?? false)
                {
                    _sshConfigDir = response.Path;
                }
            }

            return _sshConfigDir;
        }

        public async Task<string[]> GetFilesFromSshConfigDirAsync()
        {
            var response = await GetSshConfigFolderAsync(false);

            if (response == null || !response.Success)
            {
                return null;
            }

            if (string.IsNullOrEmpty(_sshConfigDir))
            {
                _sshConfigDir = response.Path;
            }

            return response.Files;
        }

        private async Task<GetSshConfigFolderResponse> GetSshConfigFolderAsync(bool includeContent)
        {
            var responseMessage =
                await _appServiceConnection.SendMessageAsync(CreateMessage(new GetSshConfigFolderRequest
                    {IncludeContent = includeContent}));

            return JsonConvert.DeserializeObject<GetSshConfigFolderResponse>(
                (string) responseMessage[MessageKeys.Content]);
        }

        public async Task<bool> CheckFileExistsAsync(string path)
        {
            var responseMessage =
                await _appServiceConnection.SendMessageAsync(CreateMessage(new CheckFileExistsRequest {Path = path}));

            return JsonConvert.DeserializeObject<CommonResponse>((string) responseMessage[MessageKeys.Content]).Success;
        }

        public void MuteTerminal(bool mute)
        {
            _appServiceConnection.SendMessageAsync(CreateMessage(new MuteTerminalRequest { Mute = mute }));
        }

        public void UpdateSettings(ApplicationSettings settings)
        {
            _appServiceConnection.SendMessageAsync(CreateMessage(new UpdateSettingsRequest { Settings = settings }));
        }

        public async Task<CreateTerminalResponse> CreateTerminal(byte id, TerminalSize size, ShellProfile shellProfile, SessionType sessionType)
        {
            var request = new CreateTerminalRequest
            {
                Id = id,
                Size = size,
                Profile = shellProfile,
                SessionType = sessionType
            };

            Logger.Instance.Debug("Sending CreateTerminalRequest: {@request}", request);

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(request));
            var response = JsonConvert.DeserializeObject<CreateTerminalResponse>((string)responseMessage[MessageKeys.Content]);

            Logger.Instance.Debug("Received CreateTerminalResponse: {@response}", response);

            return response;
        }

        public async Task<PauseTerminalOutputResponse> PauseTerminalOutput(byte id, bool pause)
        {
            var request = new PauseTerminalOutputRequest
            {
                Id = id,
                Pause = pause
            };

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(request));
            return JsonConvert.DeserializeObject<PauseTerminalOutputResponse>((string)responseMessage[MessageKeys.Content]);
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
                case Constants.TerminalBufferRequestIdentifier:
                    var terminalId = (byte)e[MessageKeys.TerminalId];

                    if (_terminalOutputHandlers.ContainsKey(terminalId))
                    {
                        _terminalOutputHandlers[terminalId].Invoke((byte[])messageContent);
                    }
                    else
                    {
                        Logger.Instance.Error("Received output for unknown terminal Id {id}", terminalId);
                    }
                    break;
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

        public Task ResizeTerminal(byte id, TerminalSize size)
        {
            var request = new ResizeTerminalRequest
            {
                TerminalId = id,
                NewSize = size
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        public void SubscribeForTerminalOutput(byte terminalId, Action<byte[]> callback)
        {
            _terminalOutputHandlers[terminalId] = callback;
        }

        public void UnsubscribeFromTerminalOutput(byte terminalId)
        {
            if (_terminalOutputHandlers.ContainsKey(terminalId))
            {
                _terminalOutputHandlers.Remove(terminalId);
            }
        }

        public Task UpdateToggleWindowKeyBindings()
        {
            var keyBindings = _settingsService.GetCommandKeyBindings()[nameof(Command.ToggleWindow)];

            var request = new SetToggleWindowKeyBindingsRequest
            {
                KeyBindings = keyBindings
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        public Task Write(byte id, byte[] data)
        {
            var message = new ValueSet
            {
                [MessageKeys.Type] = Constants.TerminalBufferRequestIdentifier,
                [MessageKeys.TerminalId] = id,
                [MessageKeys.Content] = data
            };

            return _appServiceConnection.SendMessageAsync(message);
        }

        public Task CloseTerminal(byte terminalId)
        {
            var request = new TerminalExitedRequest(terminalId, -1);

            Logger.Instance.Debug("Sending TerminalExitedRequest: {@request}", request);

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        private static readonly Dictionary<string, string> CommandPaths = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> CommandErrors = new Dictionary<string, string>();

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

            var request = new GetCommandPathRequest {Command = command};

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(request));
            var response =
                JsonConvert.DeserializeObject<StringValueResponse>((string) responseMessage[MessageKeys.Content]);

            if (response.Success)
            {
                CommandPaths[commandLower] = response.Value;

                return response.Value;
            }

            CommandErrors[commandLower] = response.Error;

            throw new Exception(response.Error);
        }

        private ValueSet CreateMessage(IMessage content)
        {
            return new ValueSet
            {
                [MessageKeys.Type] = content.Identifier,
                [MessageKeys.Content] = JsonConvert.SerializeObject(content)
            };
        }
    }
}