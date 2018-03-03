using System;
using System.Threading.Tasks;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using Windows.Web.Http;

namespace FluentTerminal.App.Services.Implementation
{
    internal class TrayProcessCommunicationService : ITrayProcessCommunicationService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;

        public TrayProcessCommunicationService(ISettingsService settingsService)
        {
            _httpClient = new HttpClient();
            _settingsService = settingsService;

            UpdateToggleWindowKeyBindings();
        }

        public Task UpdateToggleWindowKeyBindings()
        {
            var keyBindings = _settingsService.GetKeyBindings().ToggleWindow;
            return _httpClient.PostAsync(new Uri($"http://localhost:9000/keybindings/togglewindow"), HttpJsonContent.From(keyBindings)).AsTask();
        }

        public async Task<CreateTerminalResponse> CreateTerminal(TerminalSize size, ShellConfiguration shellConfiguration)
        {
            var request = new CreateTerminalRequest
            {
                Size = size,
                Configuration = shellConfiguration
            };

            var response = await _httpClient.PostAsync(new Uri($"http://localhost:9000/terminals"), HttpJsonContent.From(request));
            var createTerminalResponse = await response.Content.ReadAs<CreateTerminalResponse>();

            return createTerminalResponse;
        }

        public Task ResizeTerminal(int id, TerminalSize size)
        {
            return _httpClient.PostAsync(new Uri($"http://localhost:9000/terminals/{id}/resize"), HttpJsonContent.From(size)).AsTask();
        }
    }
}
