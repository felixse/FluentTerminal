using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var response = JsonConvert.DeserializeObject<GetAvailablePortResponse>(responseMessage[MessageKeys.Content]);

            Logger.Instance.Debug("Received GetAvailablePortResponse: {@response}", response);

            return response;
        }

        private string _userName;

        public async Task<string> GetUserName()
        {
            if (!string.IsNullOrEmpty(_userName))
                // Returning the username from cache
                return _userName;

            GetUserNameResponse response;

            // No need to crash for username, so try/catch
            try
            {
                var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(new GetUserNameRequest()));
                response = JsonConvert.DeserializeObject<GetUserNameResponse>(responseMessage[MessageKeys.Content]);
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
            IDictionary<string, string> responseMessage =
                await _appServiceConnection.SendMessageAsync(CreateMessage(new SaveTextFileRequest
                    {Path = path, Content = content}));

            CommonResponse response =
                JsonConvert.DeserializeObject<CommonResponse>(responseMessage[MessageKeys.Content]);

            if (!response.Success)
                throw new Exception(string.IsNullOrEmpty(response.Error) ? "Failed to save the file." : response.Error);
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
            var response = JsonConvert.DeserializeObject<CreateTerminalResponse>(responseMessage[MessageKeys.Content]);

            Logger.Instance.Debug("Received CreateTerminalResponse: {@response}", response);

            return response;
        }

        public void Initialize(IAppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, IDictionary<string, string> e)
        {
            var messageType = e[MessageKeys.Type];
            var messageContent = e[MessageKeys.Content];

            if (messageType == nameof(DisplayTerminalOutputRequest))
            {
                var request = JsonConvert.DeserializeObject<DisplayTerminalOutputRequest>(messageContent);

                if (_terminalOutputHandlers.ContainsKey(request.TerminalId))
                {
                    _terminalOutputHandlers[request.TerminalId].Invoke(request.Output);
                }
                else
                {
                    Logger.Instance.Error("Received output for unknown terminal Id {id}", request.TerminalId);
                }
            }
            else if (messageType == nameof(TerminalExitedRequest))
            {
                var request = JsonConvert.DeserializeObject<TerminalExitedRequest>(messageContent);
                Logger.Instance.Debug("Received TerminalExitedRequest: {@request}", request);

                TerminalExited?.Invoke(this, request.ToStatus());
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
            var request = new WriteDataRequest
            {
                TerminalId = id,
                Data = data
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        public Task CloseTerminal(int terminalId)
        {
            var request = new TerminalExitedRequest(terminalId, -1);

            Logger.Instance.Debug("Sending TerminalExitedRequest: {@request}", request);

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        private IDictionary<string, string> CreateMessage(object content)
        {
            return new Dictionary<string, string>
            {
                [MessageKeys.Type] = content.GetType().Name,
                [MessageKeys.Content] = JsonConvert.SerializeObject(content)
            };
        }
    }
}