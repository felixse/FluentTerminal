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
        private readonly Dictionary<int, Action<byte[]>> _terminalOutputHandlers;
        private int _nextTerminalId = 0;

        public event EventHandler<TerminalExitStatus> TerminalExited;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _terminalOutputHandlers = new Dictionary<int, Action<byte[]>>();
        }

        public int GetNextTerminalId()
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

            GetUserNameResponse response;

            // No need to crash for username, so try/catch
            try
            {
                var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(new GetUserNameRequest()));
                response = JsonConvert.DeserializeObject<GetUserNameResponse>((string)responseMessage[MessageKeys.Content]);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e, "Error while trying to get username.");

                return null;
            }

            Logger.Instance.Debug("Received GetUserNameResponse: {@response}", response);

            _userName = response.UserName;

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

        public async Task<CreateTerminalResponse> CreateTerminal(int id, TerminalSize size, ShellProfile shellProfile, SessionType sessionType)
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
                    var terminalId = (int)e[MessageKeys.TerminalId];

                    if (_terminalOutputHandlers.ContainsKey(terminalId))
                    {
                        _terminalOutputHandlers[terminalId].Invoke((byte[])messageContent);
                    }
                    else
                    {
                        Logger.Instance.Error("Received output for unknown terminal Id {id}", terminalId);
                    }
                    break;
                case TerminalExitedRequest.Identifier:
                    var request = JsonConvert.DeserializeObject<TerminalExitedRequest>((string)messageContent);
                    Logger.Instance.Debug("Received TerminalExitedRequest: {@request}", request);

                    TerminalExited?.Invoke(this, request.ToStatus());
                    break;
                default:
                    Logger.Instance.Error("Received unknown message type: {messageType}", messageType);
                    break;
            }
        }

        public Task ResizeTerminal(int id, TerminalSize size)
        {
            var request = new ResizeTerminalRequest
            {
                TerminalId = id,
                NewSize = size
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        public void SubscribeForTerminalOutput(int terminalId, Action<byte[]> callback)
        {
            _terminalOutputHandlers[terminalId] = callback;
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

        public Task Write(int id, byte[] data)
        {
            var message = new ValueSet
            {
                [MessageKeys.Type] = Constants.TerminalBufferRequestIdentifier,
                [MessageKeys.TerminalId] = id,
                [MessageKeys.Content] = data
            };

            return _appServiceConnection.SendMessageAsync(message);
        }

        public Task CloseTerminal(int terminalId)
        {
            var request = new TerminalExitedRequest(terminalId, -1);

            Logger.Instance.Debug("Sending TerminalExitedRequest: {@request}", request);

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
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