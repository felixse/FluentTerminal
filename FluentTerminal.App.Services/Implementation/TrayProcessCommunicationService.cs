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
        private Dictionary<int, Action<string>> _terminalOutputHandlers;

        public event EventHandler<int> TerminalExited;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _terminalOutputHandlers = new Dictionary<int, Action<string>>();
        }

        public async Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellProfile shellProfile)
        {
            var request = new CreateTerminalRequest
            {
                Size = size,
                Profile = shellProfile
            };

            var message = new Dictionary<string, string>
            {
                { MessageKeys.Type, MessageTypes.CreateTerminalRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            var responseMessage = await _appServiceConnection.SendMessageAsync(message);

            return JsonConvert.DeserializeObject<CreateTerminalResponse>(responseMessage[MessageKeys.Content]);
        }

        public Task Initialize(IAppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.MessageReceived += OnMessageReceived;
            return UpdateToggleWindowKeyBindings();
        }

        private void OnMessageReceived(object sender, IDictionary<string, string> e)
        {
            var messageType = e[MessageKeys.Type];
            var messageContent = e[MessageKeys.Content];

            if (messageType == MessageTypes.DisplayTerminalOutputRequest)
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
            else if (messageType == MessageTypes.TerminalExitedRequest)
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

            var message = new Dictionary<string, string>
            {
                { MessageKeys.Type, MessageTypes.ResizeTerminalRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            return _appServiceConnection.SendMessageAsync(message);
        }

        public void SubscribeForTerminalOutput(int terminalId, Action<string> callback)
        {
            _terminalOutputHandlers[terminalId] = callback;
        }

        public Task UpdateToggleWindowKeyBindings()
        {
            var keyBindings = _settingsService.GetKeyBindings()[Command.ToggleWindow];

            var request = new SetToggleWindowKeyBindingsRequest
            {
                KeyBindings = keyBindings
            };

            var message = new Dictionary<string, string>
            {
                { MessageKeys.Type, MessageTypes.SetToggleWindowKeyBindingsRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            return _appServiceConnection.SendMessageAsync(message);
        }

        public Task WriteText(int id, string text)
        {
            var request = new WriteTextRequest
            {
                TerminalId = id,
                Text = text
            };

            var message = new Dictionary<string, string>
            {
                { MessageKeys.Type, MessageTypes.WriteTextRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            return _appServiceConnection.SendMessageAsync(message);
        }

        public Task CloseTerminal(int terminalId)
        {
            var request = new TerminalExitedRequest
            {
                TerminalId = terminalId
            };

            var message = new Dictionary<string, string>
            {
                { MessageKeys.Type, MessageTypes.TerminalExitedRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            return _appServiceConnection.SendMessageAsync(message);
        }
    }
}