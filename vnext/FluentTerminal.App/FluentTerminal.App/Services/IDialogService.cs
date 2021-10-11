using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public enum DialogButton
    {
        // ReSharper disable once InconsistentNaming
        OK,
        Cancel
    }

    public interface IDialogService
    {
        Task<DialogButton> ShowMessageDialogAsync(string title, string content, params DialogButton[] buttons);

        Task<KeyBinding> ShowCreateKeyBindingDialog();

        Task<string> ShowInputDialogAsync(string title);

        Task<SshProfile> ShowSshConnectionInfoDialogAsync(SshProfile input = null);

        Task<ShellProfile> ShowCustomCommandDialogAsync(ShellProfile input = null);

        Task ShowAboutDialogAsync();
    }
}