using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
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
        private const string ShortcutFileFormat = @"[{{000214A0-0000-0000-C000-000000000046}}]
Prop3=19,0
[InternetShortcut]
IDList=
URL={0}
";

        private readonly ISshHelperService _sshHelperService;

        public SshInfoDialog(ISettingsService settingsService, ISshHelperService sshHelperService)
        {
            _sshHelperService = sshHelperService;
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

        private async void SaveLink_OnClick(object sender, RoutedEventArgs e)
        {
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel) DataContext;

            string error = vm.Validate(true);

            if (!string.IsNullOrEmpty(error))
            {
                await new MessageDialog(error, "Invalid Form").ShowAsync();

                return;
            }

            string content = string.Format(ShortcutFileFormat, _sshHelperService.ConvertToUri(vm));

            string fileName = string.IsNullOrEmpty(vm.Username) ? $"{vm.Host}.url" : $"{vm.Username}@{vm.Host}.url";

            FileSavePicker savePicker = new FileSavePicker {SuggestedFileName = fileName, SuggestedStartLocation = PickerLocationId.Desktop};

            savePicker.FileTypeChoices.Add("Shortcut", new List<string> {".url"});

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                await FileIO.WriteTextAsync(file, content);

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status != FileUpdateStatus.Complete)
                    await new MessageDialog($"Saving '{file.Name}' failed.", "Failed to Save").ShowAsync();
            }
        }

        private async void SshInfoDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel)DataContext;

            if (string.IsNullOrEmpty(vm.Username) || string.IsNullOrEmpty(vm.Host))
            {
                args.Cancel = true;

                await new MessageDialog("User and host are mandatory fields.", "Invalid Form").ShowAsync();

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

        private void Port_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args) =>
            args.Cancel = string.IsNullOrEmpty(args.NewText) || args.NewText.Any(c => !char.IsDigit(c));

        public async Task<ISshConnectionInfo> GetSshConnectionInfoAsync(ISshConnectionInfo input = null)
        {
            if (input != null)
                DataContext = ((SshConnectionInfoViewModel)input).Clone();

            return await ShowAsync() == ContentDialogResult.Primary ? (SshConnectionInfoViewModel) DataContext : null;
        }
    }
}
