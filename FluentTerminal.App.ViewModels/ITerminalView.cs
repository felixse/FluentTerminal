using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public interface ITerminalView
    {
        Task ChangeTheme(TerminalTheme theme);
        Task ChangeKeyBindings();
        Task ChangeOptions(TerminalOptions options);
        Task Initialize(TerminalViewModel viewModel);
        void DisposalPrepare();
        Task FindNext(string searchText);
        Task FindPrevious(string searchText);
        Task FocusTerminal();
        Task<string> SerializeXtermState();
    }
}
