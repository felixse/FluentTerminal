using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Dialogs
{
    public sealed partial class SshProfileSelectionDialog : ContentDialog, ISshProfileSelectionDialog
    {
        private IDialogService _dialogService;
        public IEnumerable<SshProfile> Profiles { get; }

        public SshProfile SelectedProfile { get; private set; }

        public SshProfileSelectionDialog(ISettingsService settingsService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            Profiles = settingsService.GetSshProfiles();
            SelectedProfile = Profiles.FirstOrDefault();
            this.InitializeComponent();
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        public async Task<SshProfile> SelectProfile()
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