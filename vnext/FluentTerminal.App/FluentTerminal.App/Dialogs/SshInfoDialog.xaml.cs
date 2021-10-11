using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class SshInfoDialog : ContentDialog, ISshConnectionInfoDialog
    {
        private readonly ISettingsService _settingsService;
        private readonly IFileSystemService _fileSystemService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        public SshConnectViewModel ViewModel { get; private set; }

        public ICommand BrowseIdentityFileCommand { get; }
        public ICommand SaveLinkCommand { get; }

        public SshInfoDialog(ISettingsService settingsService,
            IFileSystemService fileSystemService, ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _fileSystemService = fileSystemService;
            _trayProcessCommunicationService = trayProcessCommunicationService;

            InitializeComponent();

            BrowseIdentityFileCommand = new AsyncRelayCommand(BrowseIdentityFile);
            SaveLinkCommand = new AsyncRelayCommand(SaveLink);

            PrimaryButtonText = I18N.Translate("OK");
            SecondaryButtonText = I18N.Translate("Cancel");

            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        private void SetupFocus()
        {
            if (string.IsNullOrEmpty(ViewModel.Username))
            {
                UserTextBox.Focus(FocusState.Programmatic);
            }
            else if (string.IsNullOrEmpty(ViewModel.Host))
            {
                HostTextBox.Focus(FocusState.Programmatic);
            }
            else
            {
                Focus(FocusState.Programmatic);
            }
        }

        private async Task BrowseIdentityFile()
        {
            var openPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            openPicker.FileTypeFilter.Add("*");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                ViewModel.SetValidatedIdentityFile(file.Path);
            }
        }

        private async Task SaveLink()
        {
            var link = await ViewModel.GetUrlAsync();

            if (!link.Item1)
            {
                await new MessageDialog(link.Item2, I18N.Translate("InvalidInput")).ShowAsync();

                return;
            }

            var content = ProfileProviderViewModelBase.GetShortcutFileContent(link.Item2);

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = string.IsNullOrEmpty(ViewModel.Username)
                    ? $"{ViewModel.Host}.url"
                    : $"{ViewModel.Username}@{ViewModel.Host}.url"
            };

            savePicker.FileTypeChoices.Add("Shortcut", new List<string> { ".url" });

            var file = await savePicker.PickSaveFileAsync();

            if (file == null)
            {
                return;
            }

            try
            {
                await _trayProcessCommunicationService.SaveTextFileAsync(file.Path, content);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, I18N.Translate("Error")).ShowAsync();
            }
        }

        private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            var error = await ViewModel.AcceptChangesAsync();

            if (!string.IsNullOrEmpty(error))
            {
                args.Cancel = true;

                await new MessageDialog(error, I18N.Translate("InvalidInput")).ShowAsync();

                SetupFocus();
            }

            deferral.Complete();
        }

        private void Port_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args) =>
            args.Cancel = string.IsNullOrEmpty(args.NewText) || args.NewText.Any(c => !char.IsDigit(c));

        public async Task<SshProfile> GetSshConnectionInfoAsync(SshProfile input = null)
        {
            ViewModel = new SshConnectViewModel(_settingsService, _trayProcessCommunicationService,
                _fileSystemService, input);

            if (string.IsNullOrEmpty(ViewModel.Username))
            {
                ViewModel.Username = await _trayProcessCommunicationService.GetUserNameAsync();
            }

            SetupFocus();

            return await ShowAsync() == ContentDialogResult.Primary ? (SshProfile)ViewModel.Model : null;
        }
    }
}
