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
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Input;
using System.Text.RegularExpressions;
using Windows.UI.Core;
using Windows.UI.Xaml.Documents;
using FluentTerminal.Models.Enums;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;

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
        private ObservableCollection<string> _autoCompleteSuggestions;
        private ObservableCollection<ExecutedCommand> _commandsList;
        private List<string> _searchStrings;
        private string _oldText;
        private int _newSearch;
        private ProfileType _selectedProfileType;

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

            _oldText = "";
            _newSearch = -1;

            _selectedProfileType = ProfileType.New;
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

            if (_selectedProfileType == ProfileType.New)
            {
                ExecutedCommand executedCommand = _commandsList.SingleOrDefault(itm =>
                    itm.Value.Equals(((CommandProfileProviderViewModel)DataContext).Model.Name, StringComparison.CurrentCultureIgnoreCase));
                if (executedCommand != null)
                {
                    switch (executedCommand.ProfileType)
                    {
                        case ProfileType.SSH:
                        case ProfileType.Shell:
                            DataContext = new CommandProfileProviderViewModel(_settingsService, _applicationView,
                                _historyContainer,
                                executedCommand.ShellProfile, executedCommand.ProfileType != ProfileType.Shell);
                            break;
                        case ProfileType.History:
                            DataContext = new CommandProfileProviderViewModel(_settingsService, _applicationView,
                                _historyContainer,
                                ((CommandProfileProviderViewModel)DataContext).Model, executedCommand.ProfileType != ProfileType.Shell);
                            break;
                    }

                    _selectedProfileType = executedCommand.ProfileType;

                }
            }

            if (_selectedProfileType == ProfileType.SSH || _selectedProfileType == ProfileType.Shell)
            {
                deferral.Complete();

                return;
            }
            if (((CommandProfileProviderViewModel)DataContext).Command == "")
            {
                ((CommandProfileProviderViewModel)DataContext).Command =
                    ((CommandProfileProviderViewModel)DataContext).Model.Name;
            }


            var error = await ((CommandProfileProviderViewModel)DataContext).AcceptChangesAsync();

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

            _commandsList = new ObservableCollection<ExecutedCommand>();

            //string x = @"{\rtf1\ansi \b bold\b0 text.}";
            ExecutedCommand executedCommand;

            foreach (SshProfile sshShellProfile in _settingsService.GetSshProfiles())
            {
                executedCommand =
                    _commandsList.FirstOrDefault(c =>
                        string.Equals(sshShellProfile.Name, c.Value, StringComparison.Ordinal));

                if (executedCommand == null)
                {
                    executedCommand = new ExecutedCommand
                        { Value = sshShellProfile.Name, ExecutionCount = 1, LastExecution = DateTime.UtcNow, ProfileType = ProfileType.SSH, ShellProfile = sshShellProfile};
                    _commandsList.Add(executedCommand);
                }
            }

            foreach (ShellProfile shellProfile in _settingsService.GetShellProfiles())
            {
                executedCommand =
                    _commandsList.FirstOrDefault(c =>
                        string.Equals(shellProfile.Name, c.Value, StringComparison.Ordinal));

                if (executedCommand == null)
                {
                    executedCommand = new ExecutedCommand
                        { Value = shellProfile.Name, ExecutionCount = 1, LastExecution = DateTime.UtcNow, ProfileType = ProfileType.SSH, ShellProfile = shellProfile };
                    _commandsList.Add(executedCommand);
                }
            }

            foreach (ExecutedCommand commandHistory in ((CommandProfileProviderViewModel)DataContext).CommandHistoryObjectCollection.ExecutedCommands)
            {
                executedCommand =
                    _commandsList.FirstOrDefault(c =>
                        string.Equals(commandHistory.Value, c.Value, StringComparison.Ordinal));

                if (executedCommand == null)
                {
                    executedCommand = new ExecutedCommand
                        { Value = commandHistory.Value, ExecutionCount = 1, LastExecution = DateTime.UtcNow, ProfileType = ProfileType.History, ShellProfile = commandHistory.ShellProfile };
                    _commandsList.Add(executedCommand);
                }
                else
                {
                    //executedCommand.ShellProfile = commandHistory.ShellProfile;
                    executedCommand.ExecutionCount = commandHistory.ExecutionCount;
                    executedCommand.LastExecution = commandHistory.LastExecution;
                    executedCommand.ProfileType = commandHistory.ProfileType;
                }
                //if (!_commandsList.Contains(commandHistory))
                //{
                //    _commandsList.Add(commandHistory);
                //}
            }


            if (await ShowAsync() != ContentDialogResult.Primary)
            {
                return null;
            }

            vm = (CommandProfileProviderViewModel) DataContext;
            vm.SaveCommand(vm.Model.Name, _selectedProfileType, vm.Model);

            return vm.Model;
        }

        private void CommandTextBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string _newText = sender.Text.Trim();

                _newSearch = -1;

                if (_oldText != "")
                {
                    int _oldTextPosition = _newText.IndexOf(_oldText);
                    if (_oldTextPosition > -1)
                    {
                        Regex MyRegEx = new Regex(_oldText);

                        string _addedText = MyRegEx.Replace(_newText, "", 1);

                        if (_addedText.Trim() == "")
                        {
                            _newSearch = -2;
                        }
                        else if (!_addedText.Trim().Contains(" "))
                        {
                            if (_oldTextPosition == 0)
                            {
                                if (_addedText.IndexOf(" ") == 0)
                                {
                                    _searchStrings.Add(_addedText.Trim());
                                }
                                else
                                {
                                    _searchStrings[_searchStrings.Count - 1] =
                                        _searchStrings[_searchStrings.Count - 1] + _addedText.Trim();
                                }

                                _newSearch = _searchStrings.Count - 1;
                            }
                            else
                            {
                                if (_addedText.LastIndexOf(" ") == _addedText.Length - 1)
                                {
                                    _searchStrings.Insert(0, _addedText.Trim());
                                }
                                else
                                {
                                    _searchStrings[0] = _searchStrings[0] + _addedText.Trim();
                                }

                                _newSearch = 0;
                            }
                        }
                    }
                }
                _oldText = _newText;
                if (_newSearch == -1)
                {
                    _searchStrings = sender.Text.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                    _autoCompleteSuggestions = new ObservableCollection<string>();
                    if (_searchStrings.Count > 0)
                    {
                        foreach (ExecutedCommand command in _commandsList
                            .Where(itm =>
                                _searchStrings.All(k => itm.Value.Contains(k, StringComparison.CurrentCultureIgnoreCase))))
                        {
                            if (!_autoCompleteSuggestions.Contains(command.Value))
                            {
                                _autoCompleteSuggestions.Add(command.Value);
                            }
                        }
                    }
                }
                else if (_newSearch > -1)
                {
                    _autoCompleteSuggestions = new ObservableCollection<string>(_autoCompleteSuggestions.Where(itm => itm.Contains(_searchStrings[_newSearch], StringComparison.CurrentCultureIgnoreCase)));
                }

                sender.ItemsSource = _autoCompleteSuggestions;

            }
        }

        private void CommandTextBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem != null)

                CommandTextBox.Text = args.SelectedItem.ToString();

            else

                CommandTextBox.Text = sender.Text;
        }

        private void CommandTextBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                ExecutedCommand executedCommand = _commandsList.SingleOrDefault(itm =>
                    itm.Value.Equals(args.ChosenSuggestion.ToString(), StringComparison.CurrentCultureIgnoreCase));
                if (executedCommand != null)
                {
                    DataContext = new CommandProfileProviderViewModel(_settingsService, _applicationView,
                        _historyContainer,
                        executedCommand.ShellProfile, executedCommand.ProfileType != ProfileType.Shell);

                    _selectedProfileType = executedCommand.ProfileType;

                    //CommandTextBox.Text = executedCommand.Value;
                }
            }
        }

        private void RemoveHistoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            string profileName = ((Button) sender).Tag.ToString();
            if (profileName != null)
            {
                var command =
                    _commandsList.FirstOrDefault(c =>
                        string.Equals(profileName, (c.ShellProfile == null) ? c.Value : c.ShellProfile.Name, StringComparison.CurrentCultureIgnoreCase));
                
                if (command != null)
                {
                    _commandsList.Remove(command);
                }

                _autoCompleteSuggestions.Remove(profileName);

                ((CommandProfileProviderViewModel)DataContext).RemoveCommand(profileName);
                CommandTextBox.ItemsSource = _autoCompleteSuggestions;
            }
        }

        private void CommandTextBox_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (CommandTextBox.Text.Trim() == "")
                {
                    if (_searchStrings == null && _autoCompleteSuggestions == null)
                    {
                        _searchStrings = CommandTextBox.Text.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        _autoCompleteSuggestions = new ObservableCollection<string>();
                    }
                    if (_searchStrings.Count > 0)
                    {
                        foreach (ExecutedCommand command in _commandsList
                            .Where(itm =>
                                _searchStrings.All(k => itm.Value.Contains(k, StringComparison.CurrentCultureIgnoreCase))))
                        {
                            if (!_autoCompleteSuggestions.Contains(command.Value))
                            {
                                _autoCompleteSuggestions.Add(command.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (ExecutedCommand command in _commandsList)
                        {
                            if (!_autoCompleteSuggestions.Contains(command.Value))
                            {
                                _autoCompleteSuggestions.Add(command.Value);
                            }
                        }
                    }
                    CommandTextBox.ItemsSource = _autoCompleteSuggestions;
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
}
