using System;
using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public interface ITerminalView : IDisposable
    {
        Task ChangeThemeAsync(TerminalTheme theme);
        Task ChangeKeyBindingsAsync();
        Task ChangeOptionsAsync(TerminalOptions options);
        Task InitializeAsync(TerminalViewModel viewModel);
        Task ReconnectAsync();
        void DisposalPrepare();
        Task FindNextAsync(SearchRequest request);
        Task FindPreviousAsync(SearchRequest request);
        Task FocusTerminalAsync();
        Task<string> SerializeXtermStateAsync();
        void Paste(string text);
    }
}
