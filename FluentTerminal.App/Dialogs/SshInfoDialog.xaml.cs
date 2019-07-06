using System;
using System.Collections.Generic;
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
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class SshInfoDialog : ContentDialog, ISshConnectionInfoDialog
    {
        private readonly ISettingsService _settingsService;
        private readonly IApplicationView _applicationView;
        private readonly IFileSystemService _fileSystemService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IApplicationDataContainer _historyContainer;

        public SshInfoDialog(ISettingsService settingsService, IApplicationView applicationView,
            IFileSystemService fileSystemService, ITrayProcessCommunicationService trayProcessCommunicationService,
            ApplicationDataContainers containers)
        {
            _settingsService = settingsService;
            _applicationView = applicationView;
            _fileSystemService = fileSystemService;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _historyContainer = containers.HistoryContainer;

            InitializeComponent();

            PrimaryButtonText = I18N.Translate("OK");
            SecondaryButtonText = I18N.Translate("Cancel");

            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        private void SetupFocus()
        {
            SshConnectViewModel vm = (SshConnectViewModel) DataContext;

            if (vm.CommandInput)
            {
                CommandTextBox.Focus(FocusState.Programmatic);
            }
            else if (string.IsNullOrEmpty(vm.Username))
            {
                UserTextBox.Focus(FocusState.Programmatic);
            }
            else if (string.IsNullOrEmpty(vm.Host))
            {
                HostTextBox.Focus(FocusState.Programmatic);
            }
            else
            {
                Focus(FocusState.Programmatic);
            }
        }

        private async void OnLoading(FrameworkElement sender, object args)
        {
            SshConnectViewModel vm = (SshConnectViewModel) DataContext;

            if (!vm.CommandInput && string.IsNullOrEmpty(vm.Username))
            {
                vm.Username = await _trayProcessCommunicationService.GetUserName();
            }

            SetupFocus();
        }

        private async void BrowseButtonOnClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };

            openPicker.FileTypeFilter.Add("*");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                ((SshConnectViewModel) DataContext).SetValidatedIdentityFile(file.Path);
            }
        }

        private async void SaveLink_OnClick(object sender, RoutedEventArgs e)
        {
            SshConnectViewModel vm = (SshConnectViewModel) DataContext;

            var link = await vm.GetUrlAsync();

            if (!link.Item1)
            {
                await new MessageDialog(link.Item2, I18N.Translate("InvalidInput")).ShowAsync();

                return;
            }

            var content = ProfileProviderViewModelBase.GetShortcutFileContent(link.Item2);

            FileSavePicker savePicker = new FileSavePicker {SuggestedStartLocation = PickerLocationId.Desktop};

            if (!vm.CommandInput)
            {
                savePicker.SuggestedFileName = string.IsNullOrEmpty(vm.Username) ? $"{vm.Host}.url" : $"{vm.Username}@{vm.Host}.url";
            }

            savePicker.FileTypeChoices.Add("Shortcut", new List<string> {".url"});

            StorageFile file = await savePicker.PickSaveFileAsync();

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

        private async void SshInfoDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            var error = await ((SshConnectViewModel)DataContext).AcceptChangesAsync();

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
            var vm = new SshConnectViewModel(_settingsService, _applicationView, _trayProcessCommunicationService,
                _fileSystemService, _historyContainer, input);

            DataContext = vm;

            if (await ShowAsync() != ContentDialogResult.Primary)
            {
                return null;
            }

            var profile = (SshProfile) vm.Model;

            if (vm.CommandInput)
            {
                vm.SaveCommand(profile.Location, profile.Arguments);
            }

            return profile;
        }
    }
}
