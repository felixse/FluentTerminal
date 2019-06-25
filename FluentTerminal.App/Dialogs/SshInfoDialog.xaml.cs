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
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models;
using System.Collections.ObjectModel;

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

        public ObservableCollection<TabTheme> TabThemes { get; }

        public ObservableCollection<TerminalTheme> TerminalThemes { get; }


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

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            TerminalThemes = new ObservableCollection<TerminalTheme>
            {
                new TerminalTheme
                {
                    Id = Guid.Empty,
                    Name = "Default"
                }
            };
            foreach (var theme in settingsService.GetThemes())
            {
                TerminalThemes.Add(theme);
            }
        }

        private void SetupFocus()
        {
            SshProfileViewModel vm = (SshProfileViewModel)DataContext;

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
            SshProfileViewModel vm = (SshProfileViewModel)DataContext;

            if (!string.IsNullOrEmpty(vm.Username))
                return;

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
                ((SshProfileViewModel)DataContext).SetValidatedIdentityFile(file.Path);
            }
        }

        private async void SaveLink_OnClick(object sender, RoutedEventArgs e)
        {
            SshProfileViewModel vm = (SshProfileViewModel) DataContext;

            var validationResult = await vm.ValidateAsync();

            if (validationResult != SshConnectionInfoValidationResult.Valid &&
                // We may ignore empty username for links
                validationResult != SshConnectionInfoValidationResult.UsernameEmpty)
            {
                await new MessageDialog(validationResult.GetErrorString(Environment.NewLine),
                    I18N.Translate("InvalidInput")).ShowAsync();
                return;
            }

            string content = string.Format(ShortcutFileFormat, await _sshHelperService.ConvertToUriAsync(vm));

            string fileName = string.IsNullOrEmpty(vm.Username) ? $"{vm.Host}.url" : $"{vm.Username}@{vm.Host}.url";

            FileSavePicker savePicker = new FileSavePicker {SuggestedFileName = fileName, SuggestedStartLocation = PickerLocationId.Desktop};

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

            var result = await ((ISshConnectionInfo) DataContext).ValidateAsync();

            if (result != SshConnectionInfoValidationResult.Valid)
            {
                args.Cancel = true;

                await new MessageDialog(result.GetErrorString(Environment.NewLine), I18N.Translate("InvalidInput")).ShowAsync();

                SetupFocus();
            }

            deferral.Complete();
        }

        private void Port_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args) =>
            args.Cancel = string.IsNullOrEmpty(args.NewText) || args.NewText.Any(c => !char.IsDigit(c));

        public async Task<ISshConnectionInfo> GetSshConnectionInfoAsync(ISshConnectionInfo input)
        {
            DataContext = input;

            Focus(FocusState.Programmatic);

            if (await ShowAsync() != ContentDialogResult.Primary)
            {
                return null;
            }

            var vm = (SshProfileViewModel) DataContext;

            await vm.AcceptChangesAsync();

            return vm;
        }
    }
}
