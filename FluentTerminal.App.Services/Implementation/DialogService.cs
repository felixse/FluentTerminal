using System;
using System.Threading.Tasks;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services.Implementation
{
    public class DialogService : IDialogService
    {
        private readonly Func<IShellProfileSelectionDialog> _shellProfileSelectionDialogFactory;
        private readonly Func<IMessageDialog> _messageDialogFactory;
        private readonly Func<ICreateKeyBindingDialog> _createKeyBindingDialogFactory;

        public DialogService(Func<IShellProfileSelectionDialog> shellProfileSelectionDialogFactory, Func<IMessageDialog> messageDialogFactory, Func<ICreateKeyBindingDialog> createKeyBindingDialogFactory)
        {
            _shellProfileSelectionDialogFactory = shellProfileSelectionDialogFactory;
            _messageDialogFactory = messageDialogFactory;
            _createKeyBindingDialogFactory = createKeyBindingDialogFactory;
        }

        public Task<KeyBinding> ShowCreateKeyBindingDialog()
        {
            var dialog = _createKeyBindingDialogFactory();

            return dialog.CreateKeyBinding();
        }

        public Task<DialogButton> ShowMessageDialogAsnyc(string title, string content, params DialogButton[] buttons)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (buttons.Length == 0)
            {
                throw new ArgumentException("Must not be empty", nameof(buttons));
            }

            var dialog = _messageDialogFactory();
            dialog.Content = content;
            dialog.Title = title;

            foreach (var button in buttons)
            {
                dialog.AddButton(button);
            }

            return dialog.ShowAsync();
        }

        public Task<ShellProfile> ShowProfileSelectionDialogAsync()
        {
            var dialog = _shellProfileSelectionDialogFactory();

            return dialog.SelectProfile();
        }
    }
}
