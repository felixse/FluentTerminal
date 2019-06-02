using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentTerminal.App.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SshShellProfileSelectionDialog : ContentDialog, ISshShellProfileSelectionDialog
    {
        private IDialogService _dialogService;
        public IEnumerable<SshShellProfile> Profiles { get; }

        public SshShellProfile SelectedProfile { get; private set; }

        public SshShellProfileSelectionDialog(ISettingsService settingsService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            Profiles = settingsService.GetSshShellProfiles();
            SelectedProfile = Profiles.FirstOrDefault();
            this.InitializeComponent();
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        public async Task<SshShellProfile> SelectProfile()
        {
            if (Profiles.Count() == 0)
            {
                _dialogService.ShowMessageDialogAsnyc("SSH Profile Selection", "There are no saved SSH Profiles.", DialogButton.OK);
                return null;
            }
            else
            {
                var result = await ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    return SelectedProfile;
                }

                return null;
            }
        }
    }
}