using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Implementation
{
    public class TrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        private readonly ISettingsService _settingsService;
        private IAppServiceConnection _appServiceConnection;
        private readonly Dictionary<int, Action<string>> _terminalOutputHandlers;
        private int _nextTerminalId = 0;

        public event EventHandler<int> TerminalExited;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _terminalOutputHandlers = new Dictionary<int, Action<string>>();
        }

        public int GetNextTerminalId()
        {
            return _nextTerminalId++;
        }

        public async Task<GetAvailablePortResponse> GetAvailablePort()
        {
            var request = new GetAvailablePortRequest();

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(request));

            return JsonConvert.DeserializeObject<GetAvailablePortResponse>(responseMessage[MessageKeys.Content]);
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

            var responseMessage = await _appServiceConnection.SendMessageAsync(CreateMessage(request));

            return JsonConvert.DeserializeObject<CreateTerminalResponse>(responseMessage[MessageKeys.Content]);
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
                    Debug.WriteLine("output was not handled: " + request.Output);
                }
            }
            else if (messageType == nameof(TerminalExitedRequest))
            {
                var request = JsonConvert.DeserializeObject<TerminalExitedRequest>(messageContent);

                TerminalExited?.Invoke(this, request.TerminalId);
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

        public void SubscribeForTerminalOutput(int terminalId, Action<string> callback)
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

        public Task WriteText(int id, string text)
        {
            var request = new WriteTextRequest
            {
                TerminalId = id,
                Text = text
            };

            return _appServiceConnection.SendMessageAsync(CreateMessage(request));
        }

        public Task CloseTerminal(int terminalId)
        {
            var request = new TerminalExitedRequest
            {
                TerminalId = terminalId
            };

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