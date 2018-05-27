using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingsPageViewModel : ViewModelBase
    {
        private KeyBindings _keyBindings;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        public KeyBindingsPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider, ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
            AddCommand = new RelayCommand<Command>(async command => await Add(command).ConfigureAwait(false));

            Initialize(_settingsService.GetKeyBindings());
        }

        private void Initialize(KeyBindings keyBindings)
        {
            _keyBindings = keyBindings;

            NewTab = CreateViewModel(Command.NewTab, _keyBindings.NewTab);
            ConfigurableNewTab = CreateViewModel(Command.ConfigurableNewTab, _keyBindings.ConfigurableNewTab);
            ToggleWindow = CreateViewModel(Command.ToggleWindow, _keyBindings.ToggleWindow);
            NextTab = CreateViewModel(Command.NextTab, _keyBindings.NextTab);
            PreviousTab = CreateViewModel(Command.PreviousTab, _keyBindings.PreviousTab);
            CloseTab = CreateViewModel(Command.CloseTab, _keyBindings.CloseTab);
            NewWindow = CreateViewModel(Command.NewWindow, _keyBindings.NewWindow);
            ShowSettings = CreateViewModel(Command.ShowSettings, _keyBindings.ShowSettings);
            Copy = CreateViewModel(Command.Copy, _keyBindings.Copy);
            Paste = CreateViewModel(Command.Paste, _keyBindings.Paste);

            RaisePropertyChanged(nameof(NewTab));
            RaisePropertyChanged(nameof(ConfigurableNewTab));
            RaisePropertyChanged(nameof(ToggleWindow));
            RaisePropertyChanged(nameof(NextTab));
            RaisePropertyChanged(nameof(PreviousTab));
            RaisePropertyChanged(nameof(CloseTab));
            RaisePropertyChanged(nameof(NewWindow));
            RaisePropertyChanged(nameof(ShowSettings));
            RaisePropertyChanged(nameof(Copy));
            RaisePropertyChanged(nameof(Paste));
        }

        public KeyBindingsViewModel NewTab { get; private set; }
        public KeyBindingsViewModel ConfigurableNewTab { get; private set; }
        public KeyBindingsViewModel ToggleWindow { get; private set; }
        public KeyBindingsViewModel NextTab { get; private set; }
        public KeyBindingsViewModel PreviousTab { get; private set; }
        public KeyBindingsViewModel CloseTab { get; private set; }
        public KeyBindingsViewModel NewWindow { get; private set; }
        public KeyBindingsViewModel ShowSettings { get; private set; }
        public KeyBindingsViewModel Copy { get; private set; }
        public KeyBindingsViewModel Paste { get; private set; }

        public RelayCommand RestoreDefaultsCommand { get; }
        public RelayCommand<Command> AddCommand { get; }

        private Task Add(Command command)
        {
            switch (command)
            {
                case Command.ToggleWindow:
                    return ToggleWindow.Add();
                case Command.NextTab:
                    return NextTab.Add();
                case Command.PreviousTab:
                    return PreviousTab.Add();
                case Command.NewTab:
                    return NewTab.Add();
                case Command.ConfigurableNewTab:
                    return ConfigurableNewTab.Add();
                case Command.CloseTab:
                    return CloseTab.Add();
                case Command.NewWindow:
                    return NewWindow.Add();
                case Command.ShowSettings:
                    return ShowSettings.Add();
                case Command.Copy:
                    return Copy.Add();
                case Command.Paste:
                    return Paste.Add();
            }

            return Task.CompletedTask;
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to restore the default keybindings?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Initialize(_defaultValueProvider.GetDefaultKeyBindings());
                _settingsService.SaveKeyBindings(_keyBindings);
            }
        }

        private KeyBindingsViewModel CreateViewModel(Command command, ICollection<KeyBinding> keyBindings)
        {
            var viewModel = new KeyBindingsViewModel(command, keyBindings, _dialogService);
            viewModel.Edited += OnEdited;

            return viewModel;
        }

        private void OnEdited(object sender, System.EventArgs e)
        {
            _settingsService.SaveKeyBindings(_keyBindings);
            _trayProcessCommunicationService.UpdateToggleWindowKeyBindings();
        }
    }
}