using System;
using System.Threading.Tasks;
using FluentTerminal.App.Dialogs;
using FluentTerminal.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Services.Implementation
{
    public class DialogService : IDialogService
    {
        private readonly ISettingsService _settingsService;

        public DialogService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<DialogButton> ShowMessageDialogAsnyc(string title, string content, params DialogButton[] buttons)
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

        public async Task<ShellProfile> ShowProfileSelectionDialogAsync()
        {
            var dialog = new ShellProfileSelectionDialog(_settingsService);

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return dialog.SelectedProfile;
            }
            return null;
        }
    }
}
