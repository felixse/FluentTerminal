using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.ViewModels.Infrastructure;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class CustomCommandDialog : ContentDialog, ICustomCommandDialog
    {
        private readonly ISettingsService _settingsService;
        private readonly IApplicationView _applicationView;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly ICommandHistoryService _historyService;

        private ExecutedCommand _lastChosenCommand;

        // TODO: The following field is for hacking strange behavior that deletes command text after Tab selection. Consider finding a better fix.
        private string _tabSelectedCommand;

        public CommandProfileProviderViewModel ViewModel { get; private set; }

        public IAsyncCommand SaveLinkCommand { get; }

        public CustomCommandDialog(ISettingsService settingsService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, ICommandHistoryService historyService)
        {
            _settingsService = settingsService;
            _applicationView = applicationView;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _historyService = historyService;

            InitializeComponent();

            SaveLinkCommand = new AsyncCommand(SaveLink);

            PrimaryButtonText = I18N.Translate("OK");
            SecondaryButtonText = I18N.Translate("Cancel");

            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        private void SetupFocus()
        {
            CommandTextBox.Focus(FocusState.Programmatic);
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

        private async Task SaveLink()
        {
            var link = await ViewModel.GetUrlAsync();

            if (!link.Item1)
            {
                await new MessageDialog(link.Item2, I18N.Translate("InvalidInput")).ShowAsync();

                return;
            }

            var content = ProfileProviderViewModelBase.GetShortcutFileContent(link.Item2);

            var savePicker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.Desktop };

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

        private void CommandTextBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var text = sender.Text.Trim();

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.SetFilter(text);
            }
            // TODO: Else branch added for Tab-selection hack mentioned above.
            else if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(_tabSelectedCommand))
            {
                ViewModel.Command = _tabSelectedCommand;
                _tabSelectedCommand = null;
            }
        }

        private void CommandTextBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            _lastChosenCommand = (args.SelectedItem as CommandItemViewModel)?.ExecutedCommand;
        }

        private void CommandTextBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            _lastChosenCommand = null;

            if (args.ChosenSuggestion is CommandItemViewModel commandItem)
            {
                var executedCommand = ViewModel.Commands.FirstOrDefault(c =>
                    c.ExecutedCommand.Value.Equals(commandItem.ExecutedCommand.Value,
                        StringComparison.OrdinalIgnoreCase))?.ExecutedCommand;

                if (executedCommand != null)
                {
                    ViewModel.SetProfile(executedCommand.ShellProfile.Clone());
                }
            }
        }

        private void CommandTextBox_OnPreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            var command = _lastChosenCommand;

            if (e.Key == VirtualKey.Delete && !ViewModel.IsProfileCommand(command))
            {
                ViewModel.RemoveCommand(command);
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void CommandTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _tabSelectedCommand = null;

            switch (e.Key)
            {
                case VirtualKey.Down:
                case VirtualKey.Up:

                    if (!CommandTextBox.IsSuggestionListOpen)
                    {
                        CommandTextBox.IsSuggestionListOpen = true;
                    }

                    return;

                case VirtualKey.Tab:

                    if (_lastChosenCommand != null)
                    {
                        ViewModel.Command = _lastChosenCommand.Value;
                        CommandTextBox.Text = _lastChosenCommand.Value;
                        _tabSelectedCommand = _lastChosenCommand.Value;
                        CommandTextBox.IsSuggestionListOpen = false;
                        _lastChosenCommand = null;
                    }

                    e.Handled = false;

                    return;

                case VirtualKey.Enter:

                    if (string.IsNullOrWhiteSpace(CommandTextBox.Text) && !CommandTextBox.IsSuggestionListOpen)
                    {
                        CommandTextBox.IsSuggestionListOpen = true;
                    }
                    else
                    {
                        // TODO: Try to find a better way to handle [Enter] on auto-complete field.
                        // Weird way to move the focus to the primary button...

                        if (!(FocusManager.FindLastFocusableElement(this) is Control secondaryButton) ||
                            !secondaryButton.Name.Equals("SecondaryButton"))
                        {
                            return;
                        }

                        secondaryButton.Focus(FocusState.Programmatic);

                        var options = new FindNextElementOptions
                        {
                            SearchRoot = this,
                            XYFocusNavigationStrategyOverride = XYFocusNavigationStrategyOverride.Projection
                        };

                        if (FocusManager.FindNextElement(FocusNavigationDirection.Left, options) is Control primaryButton)
                        {
                            primaryButton.Focus(FocusState.Programmatic);
                        }
                    }

                    return;
            }
        }

        public async Task<ShellProfile> GetCustomCommandAsync(ShellProfile input = null)
        {
            ViewModel = new CommandProfileProviderViewModel(_settingsService, _applicationView,
                _trayProcessCommunicationService, _historyService, input);

            SetupFocus();

            if (await ShowAsync() != ContentDialogResult.Primary)
            {
                return null;
            }

            ViewModel.Model.Tag = new DelayedHistorySaver(ViewModel);

            return ViewModel.Model;
        }
    }
}
