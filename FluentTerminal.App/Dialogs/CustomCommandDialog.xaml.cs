using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class CustomCommandDialog : ContentDialog, ICustomCommandDialog
    {
        private readonly ISettingsService _settingsService;
        private readonly IApplicationView _applicationView;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IApplicationDataContainer _historyContainer;

        public CustomCommandDialog(ISettingsService settingsService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, ApplicationDataContainers containers)
        {
            _settingsService = settingsService;
            _applicationView = applicationView;
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
            CommandTextBox.Focus(FocusState.Programmatic);
        }

        private void OnLoading(FrameworkElement sender, object args)
        {
            SetupFocus();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            var error = await((CommandProfileProviderViewModel)DataContext).AcceptChangesAsync();

            if (!string.IsNullOrEmpty(error))
            {
                args.Cancel = true;

                await new MessageDialog(error, I18N.Translate("InvalidInput")).ShowAsync();

                SetupFocus();
            }

            deferral.Complete();
        }

        private async void SaveLink_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (CommandProfileProviderViewModel) DataContext;

            var link = await vm.GetUrlAsync();

            if (!link.Item1)
            {
                await new MessageDialog(link.Item2, I18N.Translate("InvalidInput")).ShowAsync();

                return;
            }

            var content = ProfileProviderViewModelBase.GetShortcutFileContent(link.Item2);

            FileSavePicker savePicker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.Desktop };

            savePicker.FileTypeChoices.Add("Shortcut", new List<string> { ".url" });

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

        public async Task<ShellProfile> GetCustomCommandAsync(ShellProfile input = null)
        {
            var vm = new CommandProfileProviderViewModel(_settingsService, _applicationView, _historyContainer, input);

            DataContext = vm;

            if (await ShowAsync() != ContentDialogResult.Primary)
            {
                return null;
            }

            vm.SaveCommand(vm.Model.Location, vm.Model.Arguments);

            return vm.Model;
        }
    }
}
