using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace FluentTerminal.App.Services.Implementation
{
    internal class DialogService : IDialogService
    {
        public async Task<DialogButton> ShowDialogAsnyc(string title, string content, params DialogButton[] buttons)
        {
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
