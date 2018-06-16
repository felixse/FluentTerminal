using System.Collections.Generic;
using System.Threading.Tasks;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using Newtonsoft.Json;

namespace FluentTerminal.App.Services.Implementation
{
    public class TrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        private readonly ISettingsService _settingsService;
        private IAppServiceConnection _appServiceConnection;

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
            return UpdateToggleWindowKeyBindings();
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
    }
}
