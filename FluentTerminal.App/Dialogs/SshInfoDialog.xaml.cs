using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
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

        private async void OnLoading(FrameworkElement sender, object args)
        {
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel)DataContext;
            vm.Username = await GetUsername();
        }

        private static async Task<string> GetUsername()
        {
            // FindAllAsync seems to return users associated with just the
            // current session. There's usually just one but there are
            // apparently ways for multiple users to share a session. This is
            // poorly documented however.
            // Somewhat related: https://docs.microsoft.com/en-us/gaming/xbox-live/using-xbox-live/auth/retrieving-windows-system-user-on-uwp
            var users = await User.FindAllAsync();
            var user = users.FirstOrDefault();

            string domainWithUser = (string)await user.GetPropertyAsync(KnownUserProperties.DomainName); 
            if (String.IsNullOrEmpty(domainWithUser))  {
                // Fallback for non-domain account
                string accountName = ((string)await user.GetPropertyAsync(KnownUserProperties.AccountName)).ToLower();
                if (String.IsNullOrEmpty(accountName))
                {
                    return ((string)await user.GetPropertyAsync(KnownUserProperties.DisplayName)).ToLower();
                }
                return accountName;
            }
            return domainWithUser.Split(@"\", 1).Last(); // @"domain\user" form 
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
