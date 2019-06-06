﻿using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public enum DialogButton
    {
        OK,
        Cancel
    }

    public interface IDialogService
    {
        Task<DialogButton> ShowMessageDialogAsnyc(string title, string content, params DialogButton[] buttons);

        Task<ShellProfile> ShowProfileSelectionDialogAsync();

        Task<KeyBinding> ShowCreateKeyBindingDialog();
        Task<string> ShowInputDialogAsync(string title);

        Task<ISshConnectionInfo> ShowSshConnectionInfoDialogAsync(ISshConnectionInfo input);

        Task<SshProfile> ShowSshProfileSelectionDialogAsync();
    }
}