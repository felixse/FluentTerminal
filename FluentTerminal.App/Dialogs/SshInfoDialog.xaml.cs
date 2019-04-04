using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Services.Dialogs;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class SshInfoDialog : ContentDialog, ISshConnectionInfoDialog
    {
        public SshInfoDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public async Task<ISshConnectionInfo> GetSshConnectionInfoAsync()
        {
            ContentDialogResult result = await ShowAsync();

            return result == ContentDialogResult.Primary ? (ISshConnectionInfo)DataContext : null;
        }
    }
}
