using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.Models;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Implementation
{
    public class DialogService : IDialogService
    {
        private readonly Func<IShellProfileSelectionDialog> _shellProfileSelectionDialogFactory;
        private readonly Func<IMessageDialog> _messageDialogFactory;
        private readonly Func<ICreateKeyBindingDialog> _createKeyBindingDialogFactory;
        private readonly Func<IInputDialog> _inputDialogFactory;
        private readonly Func<ISshConnectionInfoDialog> _sshConnectionInfoDialogFactory;
        private readonly Func<IQuickSshDialog> _quickSshDialogFactory;
        private readonly Func<ISshProfileSelectionDialog> _sshProfileSelectionDialogFactory;

        public DialogService(Func<IShellProfileSelectionDialog> shellProfileSelectionDialogFactory,
            Func<IMessageDialog> messageDialogFactory, Func<ICreateKeyBindingDialog> createKeyBindingDialogFactory,
            Func<IInputDialog> inputDialogFactory, Func<ISshConnectionInfoDialog> sshConnectionInfoDialogFactory,
            Func<IQuickSshDialog> quickSshDialogFactory,
            Func<ISshProfileSelectionDialog> sshProfileSelectionDialogFactory)
        {
            _shellProfileSelectionDialogFactory = shellProfileSelectionDialogFactory;
            _messageDialogFactory = messageDialogFactory;
            _createKeyBindingDialogFactory = createKeyBindingDialogFactory;
            _inputDialogFactory = inputDialogFactory;
            _sshConnectionInfoDialogFactory = sshConnectionInfoDialogFactory;
            _quickSshDialogFactory = quickSshDialogFactory;
            _sshProfileSelectionDialogFactory = sshProfileSelectionDialogFactory;
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

        public Task<string> ShowInputDialogAsync(string title)
        {
            var dialog = _inputDialogFactory.Invoke();
            dialog.SetTitle(title);

            return dialog.GetInput();
        }

        public Task<ShellProfile> ShowProfileSelectionDialogAsync()
        {
            var dialog = _shellProfileSelectionDialogFactory();

            return dialog.SelectProfile();
        }

        public Task<SshProfile> ShowSshConnectionInfoDialogAsync(SshProfile input = null) =>
            _sshConnectionInfoDialogFactory().GetSshConnectionInfoAsync(input);

        public Task<SshProfile> ShowQuickSshDialogAsync(SshProfile input = null) =>
            _quickSshDialogFactory().GetSshProfileAsync(input);

        public Task<SshProfile> ShowSshProfileSelectionDialogAsync()
        {
            var dialog = _sshProfileSelectionDialogFactory();

            return dialog.SelectProfile();
        }
    }
}