using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Dialogs
{
    public sealed partial class SshProfileSelectionDialog : ContentDialog, ISshProfileSelectionDialog
    {
        private IDialogService _dialogService;

        public ObservableCollection<SshProfile> Profiles { get; }

        public SshProfile SelectedProfile { get; private set; }

        public SshProfileSelectionDialog(ISettingsService settingsService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            Profiles = new ObservableCollection<SshProfile>(settingsService.GetSshProfiles().OrderBy(p => p.Name));

            var selectedProfile = settingsService.GetDefaultSshProfile();

            if (selectedProfile != null)
            {
                selectedProfile = Profiles.FirstOrDefault(p => p.Id.Equals(selectedProfile.Id));
            }

            SelectedProfile = selectedProfile ?? Profiles.FirstOrDefault();

            this.InitializeComponent();
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        public async Task<SshProfile> SelectProfile()
        {
            if (!Profiles.Any())
            {
                await _dialogService.ShowMessageDialogAsnyc("SSH Profile Selection", "There are no saved SSH Profiles.",
                    DialogButton.OK);
                return null;
            }

            return await ShowAsync() == ContentDialogResult.Primary ? SelectedProfile : null;
        }
    }
}