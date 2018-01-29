using FluentTerminal.App.Models;
using FluentTerminal.App.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel
    {
        private static HttpClient _httpClient;
        private ITerminalView _terminalView;

        public int Id { get; private set; }

        static TerminalViewModel()
        {
            _httpClient = new HttpClient();
        }

        public async Task OnViewIsReady(ITerminalView terminalView)
        {
            _terminalView = terminalView;
            _terminalView.TerminalSizeChanged += OnTerminalSizeChanged;
            _terminalView.TerminalTitleChanged += OnTerminalTitleChanged;

            var size = await _terminalView.CreateTerminal(null);

            var response = await _httpClient.PostAsync(new Uri($"http://localhost:9000/terminals?cols={size.Columns}&rows={size.Rows}"), null);
            var url = await response.Content.ReadAsStringAsync();
            Id = int.Parse(url.Split(":")[2].Trim('"'));

            await _terminalView.ConnectToSocket(url);
        }

        private void OnTerminalTitleChanged(object sender, string e)
        {
            
        }

        private async void OnTerminalSizeChanged(object sender, TerminalSize e)
        {
            Debug.WriteLine("size changed");
            await _httpClient.PostAsync(new Uri($"http://localhost:9000/terminals/{Id}/size?cols={e.Columns}&rows={e.Rows}"), null);
        }
    }
}
