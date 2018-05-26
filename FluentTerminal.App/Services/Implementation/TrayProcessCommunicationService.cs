using System;
using System.Threading.Tasks;
using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using Newtonsoft.Json;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FluentTerminal.App.Services.Implementation
{
    internal class TrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        private readonly ISettingsService _settingsService;
        private AppServiceConnection _appServiceConnection;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellProfile shellProfile)
        {
            var request = new CreateTerminalRequest
            {
                Size = size,
                Profile = shellProfile
            };

            var message = new ValueSet
            {
                { MessageKeys.Type, MessageTypes.CreateTerminalRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            var responseMessage = await _appServiceConnection.SendMessageAsync(message);

            return JsonConvert.DeserializeObject<CreateTerminalResponse>((string)responseMessage.Message[MessageKeys.Content]);
        }

        public Task Initialize(AppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            return UpdateToggleWindowKeyBindings();
        }

        public Task ResizeTerminal(int id, TerminalSize size)
        {
            var request = new ResizeTerminalRequest
            {
                TerminalId = id,
                NewSize = size
            };

            var message = new ValueSet
            {
                { MessageKeys.Type, MessageTypes.ResizeTerminalRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            return _appServiceConnection.SendMessageAsync(message).AsTask();
        }

        public Task UpdateToggleWindowKeyBindings()
        {
            var keyBindings = _settingsService.GetKeyBindings().ToggleWindow;

            var request = new SetToggleWindowKeyBindingsRequest
            {
                KeyBindings = keyBindings
            };

            var message = new ValueSet
            {
                { MessageKeys.Type, MessageTypes.SetToggleWindowKeyBindingsRequest },
                { MessageKeys.Content, JsonConvert.SerializeObject(request) }
            };

            return _appServiceConnection.SendMessageAsync(message).AsTask();
        }
    }
}
