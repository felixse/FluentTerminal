using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class ShellProfileSelectionDialog : ContentDialog, IShellProfileSelectionDialog
    {
        public ObservableCollection<ShellProfile> Profiles { get; }

        public ShellProfile SelectedProfile { get; private set; }

        public ShellProfileSelectionDialog(ISettingsService settingsService)
        {
            Profiles = new ObservableCollection<ShellProfile>(settingsService.GetAllProfiles().OrderBy(p => p.Name));

            SelectedProfile = Profiles.First();

            InitializeComponent();

            PrimaryButtonText = I18N.TranslateWithFallback("OK", "OK");
            SecondaryButtonText = I18N.TranslateWithFallback("Cancel", "Cancel");

            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        public Task<ShellProfile> SelectProfileAsync()
        {
            return ShowAsync().AsTask()
                .ContinueWith(t => t.Result == ContentDialogResult.Primary ? SelectedProfile : null,
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}