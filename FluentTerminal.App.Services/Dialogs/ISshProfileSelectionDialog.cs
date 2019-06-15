using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface ISshProfileSelectionDialog
    {
        SshProfile SelectedProfile { get; }

        Task<SshProfile> SelectProfile();
    }
}
