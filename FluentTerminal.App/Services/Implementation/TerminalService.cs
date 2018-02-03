using System;
using System.Threading.Tasks;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using Windows.Web.Http;

namespace FluentTerminal.App.Services.Implementation
{
    internal class TerminalService : ITerminalService
    {
        private readonly HttpClient _httpClient;

        public TerminalService()
        {
            _httpClient = new HttpClient();
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
