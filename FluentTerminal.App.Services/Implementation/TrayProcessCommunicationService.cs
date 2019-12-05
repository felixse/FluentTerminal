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
        private readonly ISettingsService _settingsService;
        private IAppServiceConnection _appServiceConnection;
        private readonly Dictionary<byte, Action<byte[]>> _terminalOutputHandlers;
        // ReSharper disable once RedundantDefaultMemberInitializer
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

        private string _userName;

        public Task<string> GetUserNameAsync()
        {
            if (!string.IsNullOrEmpty(_userName))
            {
                // Returning the username from cache
                return Task.FromResult(_userName);
            }

            return _appServiceConnection.SendMessageAsync(CreateMessage(new GetUserNameRequest()))
                .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Logger.Instance.Error(t.Exception, "Error while trying to get username.");
                        }
                        else if (t.Status == TaskStatus.RanToCompletion)
                        {
                            var response =
                                JsonConvert.DeserializeObject<StringValueResponse>((string) t.Result[MessageKeys.Content]);

                            Logger.Instance.Debug("Received GetUserNameResponse: {@response}", response);

                            if (response.Success)
                            {
                                _userName = response.Value;
                            }
                        }

                        return _userName;
                    });
        }

        public Task SaveTextFileAsync(string path, string content)
        {
            return _appServiceConnection
                .SendMessageAsync(CreateMessage(new SaveTextFileRequest {Path = path, Content = content}))
                .ContinueWith(
                    t =>
                    {
                        if (t.Result == null)
                        {
                            // Won't happen ever, but...
                            throw new SaveTextFileException("Missing tray process response.");
                        }

                        var response =
                            JsonConvert.DeserializeObject<CommonResponse>((string) t.Result[MessageKeys.Content]);

                        if (!response.Success)
                        {
                            throw new SaveTextFileException(string.IsNullOrEmpty(response.Error)
                                ? "Failed to save the file."
                                : response.Error);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private static string _sshConfigDir;

        public Task<string> GetSshConfigDirAsync()
        {
            if (!string.IsNullOrEmpty(_sshConfigDir))
            {
                return Task.FromResult(_sshConfigDir);
            }

            return GetSshConfigFolderAsync(false).ContinueWith(t =>
            {
                if (t.Result?.Success ?? false)
                {
                    _sshConfigDir = t.Result.Path;
                }

                return _sshConfigDir;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<string[]> GetFilesFromSshConfigDirAsync()
        {
            return GetSshConfigFolderAsync(false).ContinueWith(t =>
            {
                if (t.Result?.Success ?? false)
                {
                    _sshConfigDir = t.Result.Path;

                    return t.Result.Files;
                }

                return null as string[];
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<GetSshConfigFolderResponse> GetSshConfigFolderAsync(bool includeContent)
        {
            return _appServiceConnection
                .SendMessageAsync(CreateMessage(new GetSshConfigFolderRequest {IncludeContent = includeContent}))
                .ContinueWith(
                    t => t.Result == null
                        ? null as GetSshConfigFolderResponse
                        : JsonConvert.DeserializeObject<GetSshConfigFolderResponse>(
                            (string) t.Result[MessageKeys.Content]), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<bool> CheckFileExistsAsync(string path)
        {
            return _appServiceConnection.SendMessageAsync(CreateMessage(new CheckFileExistsRequest {Path = path}))
                .ContinueWith(
                    t => t.Result != null && JsonConvert
                             .DeserializeObject<CommonResponse>((string) t.Result[MessageKeys.Content]).Success,
                    TaskContinuationOptions.OnlyOnRanToCompletion);

        }

        public Task MuteTerminalAsync(bool mute)
        {
            return _appServiceConnection.SendMessageAsync(CreateMessage(new MuteTerminalRequest {Mute = mute}));
        }

        public void UpdateSettings(ApplicationSettings settings)
        {
            _appServiceConnection.SendMessageAsync(CreateMessage(new UpdateSettingsRequest { Settings = settings }));
        }

        public Task<CreateTerminalResponse> CreateTerminalAsync(byte id, TerminalSize size, ShellProfile shellProfile,
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

            return _appServiceConnection.SendMessageAsync(CreateMessage(request))
                .ContinueWith(t =>
                {
                    var response =
                        JsonConvert.DeserializeObject<CreateTerminalResponse>(
                            (string) t.Result[MessageKeys.Content]);

                    Logger.Instance.Debug("Received CreateTerminalResponse: {@response}", response);

                    return response;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<PauseTerminalOutputResponse> PauseTerminalOutputAsync(byte id, bool pause)
        {
            var request = new PauseTerminalOutputRequest
            {
                Id = id,
                Pause = pause
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request)).ContinueWith(
                t => JsonConvert.DeserializeObject<PauseTerminalOutputResponse>((string) t.Result[MessageKeys.Content]),
                TaskContinuationOptions.OnlyOnRanToCompletion);
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

        public Task ResizeTerminalAsync(byte id, TerminalSize size)
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

        public void UnsubscribeFromTerminalOutput(byte id)
        {
            if (_terminalOutputHandlers.ContainsKey(id))
            {
                _terminalOutputHandlers.Remove(id);
            }
        }

        public Task UpdateToggleWindowKeyBindingsAsync()
        {
            var keyBindings = _settingsService.GetCommandKeyBindings()[nameof(Command.ToggleWindow)];

            var request = new SetToggleWindowKeyBindingsRequest
            {
                KeyBindings = keyBindings
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
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

        private static readonly Dictionary<string, string> CommandPaths = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> CommandErrors = new Dictionary<string, string>();

        public Task<string> GetCommandPathAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Input value is null or empty.", nameof(command));
            }

            command = command.Trim();

            var commandLower = command.ToLowerInvariant();

            if (CommandPaths.TryGetValue(commandLower, out var path))
            {
                return Task.FromResult(path);
            }

            if (CommandErrors.TryGetValue(commandLower, out var error))
            {
                throw new Exception(error);
            }

            return _appServiceConnection.SendMessageAsync(CreateMessage(new GetCommandPathRequest {Command = command}))
                .ContinueWith(t =>
                    {
                        var response =
                            JsonConvert.DeserializeObject<StringValueResponse>(
                                (string) t.Result[MessageKeys.Content]);

                        if (response.Success)
                        {
                            CommandPaths[commandLower] = response.Value;

                            return response.Value;
                        }

                        CommandErrors[commandLower] = response.Error;

                        throw new Exception(response.Error);
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task QuitApplicationAsync()
        {
            return _appServiceConnection.SendMessageAsync(CreateMessage(new QuitApplicationRequest()));
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