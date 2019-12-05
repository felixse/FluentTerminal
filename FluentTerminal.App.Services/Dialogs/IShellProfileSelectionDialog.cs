using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface IShellProfileSelectionDialog
    {
        ShellProfile SelectedProfile { get; }

        Task<ShellProfile> SelectProfileAsync();
    }
}