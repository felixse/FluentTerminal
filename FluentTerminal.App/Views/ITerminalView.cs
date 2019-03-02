using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views
{
    public interface ITerminalView
    {
        Task ChangeTheme(TerminalTheme theme);
        Task ChangeKeyBindings();
        Task ChangeOptions(TerminalOptions options);
        Task Initialize(TerminalViewModel viewModel);
        Task FindNext(string searchText);
        Task FindPrevious(string searchText);
        Task FocusTerminal();
    }
}
