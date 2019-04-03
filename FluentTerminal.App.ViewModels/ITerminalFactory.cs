using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels
{
    public interface ITerminalFactory
    {
        // Has to be called from the dispatcher thread!
        Task<TerminalViewModel> InitializeTerminalAsync(ShellProfile profile);
    }
}