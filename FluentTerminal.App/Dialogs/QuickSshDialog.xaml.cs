using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class QuickSshDialog : ContentDialog, IQuickSshDialog
    {
        private readonly ISettingsService _settingsService;
        private readonly IApplicationView _applicationView;

        public QuickSshDialog(ISettingsService settingsService, IApplicationView applicationView)
        {
            _settingsService = settingsService;
            _applicationView = applicationView;

            InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            var error = await((QuickSshViewModel)DataContext).ValidateAsync();

            if (string.IsNullOrEmpty(error))
            {
                await ((QuickSshViewModel) DataContext).AcceptChangesAsync();
            }
            else
            {
                args.Cancel = true;

                await new MessageDialog(error, I18N.Translate("InvalidInput")).ShowAsync();

                CommandTextBox.Focus(FocusState.Programmatic);
            }

            deferral.Complete();
        }

        private void OnLoading(FrameworkElement sender, object args)
        {
            CommandTextBox.Focus(FocusState.Programmatic);
        }

        public async Task<SshProfile> GetSshProfileAsync(SshProfile input = null)
        {
            using (var vm = new QuickSshViewModel(_settingsService, _applicationView, input))
            {
                DataContext = vm;

                return (await ShowAsync() == ContentDialogResult.Primary) ? (SshProfile) vm.GetProfile() : null;
            }
        }
    }
}
