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
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models.Enums;
using FluentTerminal.App.Services.Utilities;

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
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        public IEnumerable<LineEndingStyle> LineEndingStyles { get; } = (LineEndingStyle[])Enum.GetValues(typeof(LineEndingStyle));

        public SshInfoDialog(ISettingsService settingsService, ISshHelperService sshHelperService,
            ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _sshHelperService = sshHelperService;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            InitializeComponent();
            PrimaryButtonText = I18N.Translate("OK");
            SecondaryButtonText = I18N.Translate("Cancel");
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        private void SetupFocus()
        {
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel)DataContext;

            if (string.IsNullOrEmpty(vm.Username))
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
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel)DataContext;

            if (!string.IsNullOrEmpty(vm.Username))
            {
                return;
            }

            vm.Username = await _trayProcessCommunicationService.GetUserName();

            SetupFocus();
        }

        private async void BrowseButtonOnClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };

            openPicker.FileTypeFilter.Add("*");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                ((SshConnectionInfoViewModel)DataContext).IdentityFile = file.Path;
            }
        }

        private async void SaveLink_OnClick(object sender, RoutedEventArgs e)
        {
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel) DataContext;

            var validationResult = vm.Validate(true);

            if (validationResult != SshConnectionInfoValidationResult.Valid)
            {
                await new MessageDialog(I18N.Translate($"{nameof(SshConnectionInfoValidationResult)}.{validationResult}"), I18N.Translate("InvalidInput")).ShowAsync();

                return;
            }

            var content = string.Format(ShortcutFileFormat, _sshHelperService.ConvertToUri(vm));

            var fileName = string.IsNullOrEmpty(vm.Username) ? $"{vm.Host}.url" : $"{vm.Username}@{vm.Host}.url";

            var savePicker = new FileSavePicker {SuggestedFileName = fileName, SuggestedStartLocation = PickerLocationId.Desktop};

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
            SshConnectionInfoViewModel vm = (SshConnectionInfoViewModel)DataContext;

            if (string.IsNullOrEmpty(vm.Username) || string.IsNullOrEmpty(vm.Host))
            {
                args.Cancel = true;

                await new MessageDialog(I18N.Translate("UserAndHostMandatory"), I18N.Translate("InvalidInput")).ShowAsync();

                SetupFocus();
                return;
            }

            if (vm.SshPort == 0)
            {
                args.Cancel = true;

                await new MessageDialog(I18N.Translate("PortCannotBeZero"), I18N.Translate("InvalidInput")).ShowAsync();

                SetupFocus();
                return;
            }

            args.Cancel = false;
        }

        private void Port_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args) =>
            args.Cancel = string.IsNullOrEmpty(args.NewText) || args.NewText.Any(c => !char.IsDigit(c));

        public async Task<ISshConnectionInfo> GetSshConnectionInfoAsync(ISshConnectionInfo input = null)
        {
            if (input != null)
            {
                DataContext = ((SshConnectionInfoViewModel)input).Clone();
            }

            this.Focus(FocusState.Programmatic);
            return await ShowAsync() == ContentDialogResult.Primary ? (SshConnectionInfoViewModel) DataContext : null;
        }
    }
}
