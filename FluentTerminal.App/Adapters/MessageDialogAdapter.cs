using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Utilities;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace FluentTerminal.App.Adapters
{
    public class MessageDialogAdapter : IMessageDialog
    {
        private readonly MessageDialog _messageDialog;

        public MessageDialogAdapter()
        {
            _messageDialog = new MessageDialog(string.Empty);
        }

        public string Title
        {
            get => _messageDialog.Title;
            set => _messageDialog.Title = value;
        }

        public string Content
        {
            get => _messageDialog.Content;
            set => _messageDialog.Content = value;
        }

        public void AddButton(DialogButton button)
        {
            var command = new UICommand
            {
                Label = I18N.Translate(button.ToString()),
                Id = button
            };
            _messageDialog.Commands.Add(command);
        }

        public async Task<DialogButton> ShowAsync()
        {
            var result = await _messageDialog.ShowAsync();

            return (DialogButton)result.Id;
        }
    }
}