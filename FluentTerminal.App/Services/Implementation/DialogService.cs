using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace FluentTerminal.App.Services.Implementation
{
    public class DialogService : IDialogService
    {
        public async Task<DialogButton> ShowDialogAsnyc(string title, string content, params DialogButton[] buttons)
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

            var dialog = new MessageDialog(content, title);

            foreach (var button in buttons)
            {
                dialog.Commands.Add(new UICommand(button.ToString()));
            }

            var result = await dialog.ShowAsync();

            return Enum.Parse<DialogButton>(result.Label);
        }
    }
}
