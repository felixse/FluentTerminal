using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface ISshShellProfileSelectionDialog
    {
        SshShellProfile SelectedProfile { get; }

        Task<SshShellProfile> SelectProfile();
    }
}
