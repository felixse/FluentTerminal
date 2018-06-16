using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
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
            _messageDialog.Commands.Add(new UICommand(button.ToString()));
        }

        public async Task<DialogButton> ShowAsync()
        {
            var result = await _messageDialog.ShowAsync();

            return Enum.Parse<DialogButton>(result.Label);
        }
    }
}