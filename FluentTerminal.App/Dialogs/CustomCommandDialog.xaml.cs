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
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models.Enums;

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
        private string _oldText;

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
            var vm = (CommandProfileProviderViewModel) DataContext;

            if (vm.ProfileType == ProfileType.Ssh || vm.ProfileType == ProfileType.Shell)
            {
                return;
            }

            var deferral = args.GetDeferral();

            var error = await vm.AcceptChangesAsync();

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
            var vm = (CommandProfileProviderViewModel)DataContext;

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

            vm = (CommandProfileProviderViewModel) DataContext;
            vm.SaveCommand(vm.Model.Name, vm.Model);

            return vm.Model;
        }

        private void CommandTextBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
            }

            var newText = sender.Text.Trim();

            if (newText.NullableEqualTo(_oldText, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _oldText = newText;

            var words = newText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var command in ((CommandProfileProviderViewModel) DataContext).Commands)
            {
                command.SetFilter(newText, words);
            }
        }

        private void CommandTextBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is CommandItemViewModel commandItem)
            {
                CommandTextBox.Text = commandItem.ExecutedCommand.Value;
            }
        }

        private void CommandTextBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is CommandItemViewModel commandItem)
            {
                ExecutedCommand executedCommand = ((CommandProfileProviderViewModel) DataContext).Commands
                    .FirstOrDefault(c =>
                        c.ExecutedCommand.Value.Equals(commandItem.ExecutedCommand.Value.ToString(),
                            StringComparison.OrdinalIgnoreCase))?.ExecutedCommand;

                if (executedCommand != null)
                {
                    ((CommandProfileProviderViewModel) DataContext).SetProfile(executedCommand.ProfileType,
                        executedCommand.ShellProfile.Clone());
                }
            }
        }

        private void RemoveHistoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is ExecutedCommand command)
            {
                ((CommandProfileProviderViewModel)DataContext).RemoveCommand(command);
            }
        }

        private void CommandTextBox_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter)
                return;

            if (string.IsNullOrWhiteSpace(CommandTextBox.Text))
            {
                CommandTextBox.IsSuggestionListOpen = true;
            }
            else
            {
                DependencyObject candidate = null;

                var options = new FindNextElementOptions()
                {
                    SearchRoot = this,
                    XYFocusNavigationStrategyOverride = XYFocusNavigationStrategyOverride.Projection
                };


                candidate =
                    FocusManager.FindLastFocusableElement(this);

                if (candidate != null && candidate is Control)
                {
                    if (((Control) candidate).Name == "SecondaryButton")
                    {
                        ((Control) candidate).Focus(FocusState.Keyboard);

                        candidate =
                            FocusManager.FindNextElement(FocusNavigationDirection.Left, options);

                        if (candidate != null && candidate is Control)
                        {
                            if (((Control) candidate).Name == "PrimaryButton")
                            {
                                ((Control) candidate).Focus(FocusState.Keyboard);

                                //var key = Key.A;                    // Key to send  
                                //var routedEvent = Keyboard.KeyDownEvent; // Event to send
                                //myText.RaiseEvent(
                                //    new KeyEventArgs(
                                //            Keyboard.PrimaryDevice,
                                //            PresentationSource.FromVisual(myText),
                                //            0,
                                //            key)
                                //        { RoutedEvent = routedEvent }
                                //);
                            }
                        }
                    }
                }
            }
        }
    }
}
