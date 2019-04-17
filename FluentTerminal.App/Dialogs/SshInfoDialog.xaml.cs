using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels;

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class SshInfoDialog : ContentDialog, ISshConnectionInfoDialog
    {
        public SshInfoDialog(ISettingsService settingsService)
        {
            InitializeComponent();
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        private async void BrowseButtonOnClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };

            openPicker.FileTypeFilter.Add("*");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
                ((SshConnectionInfoViewModel)DataContext).IdentityFile = file.Path;
        }

        private async void SshInfoDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel)DataContext;

            if (string.IsNullOrEmpty(vm.Username) || string.IsNullOrEmpty(vm.Host))
            {
                args.Cancel = true;

                MessageDialog d = new MessageDialog("User and host are mandatory fields.", "Invalid Form");

                await d.ShowAsync();

                return;
            }

            if (vm.SshPort == 0)
            {
                args.Cancel = true;

                MessageDialog d = new MessageDialog("Port cannot be 0.", "Invalid Form");

                await d.ShowAsync();

                return;
            }

            args.Cancel = false;
        }

        private void SshPort_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args) =>
            args.Cancel = string.IsNullOrEmpty(args.NewText) || args.NewText.Any(c => !char.IsDigit(c));

        public async Task<ISshConnectionInfo> GetSshConnectionInfoAsync() =>
            await ShowAsync() == ContentDialogResult.Primary ? (ISshConnectionInfo) DataContext : null;
    }
}
