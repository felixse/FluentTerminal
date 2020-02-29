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
        void DisposalPrepare();
        Task FindNextAsync(SearchRequest request);
        Task FindPreviousAsync(SearchRequest request);
        Task FocusTerminalAsync();
        Task<string> SerializeXtermStateAsync();
        Task PasteAsync(string text);
    }
}
