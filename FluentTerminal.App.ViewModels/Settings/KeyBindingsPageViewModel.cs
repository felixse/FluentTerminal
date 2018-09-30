using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingsPageViewModel : ViewModelBase
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private IDictionary<Command, ICollection<KeyBinding>> _keyBindings;

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

        public RelayCommand<Command> AddCommand { get; }
        public ObservableCollection<KeyBindingsViewModel> KeyBindings { get; } = new ObservableCollection<KeyBindingsViewModel>();
        public RelayCommand RestoreDefaultsCommand { get; }

        private async Task Add(Command command)
        {
            var keyBinding = KeyBindings.FirstOrDefault(k => k.Command == command);
            await keyBinding?.Add();
        }

        private void ClearKeyBindings()
        {
            foreach (var keyBinding in KeyBindings)
            {
                keyBinding.Edited -= OnEdited;
            }

            KeyBindings.Clear();
        }

        private void Initialize(IDictionary<Command, ICollection<KeyBinding>> keyBindings)
        {
            _keyBindings = keyBindings;

            ClearKeyBindings();

            foreach (var value in Enum.GetValues(typeof(AppCommand)))
            {
                var command = (AppCommand)value;
                var viewModel = new KeyBindingsViewModel(command, _keyBindings[command], _dialogService);
                viewModel.Edited += OnEdited;
                KeyBindings.Add(viewModel);
            }
        }

        private void OnEdited(Command command, ICollection<KeyBinding> keyBindings)
        {
            _settingsService.SaveKeyBindings(command, keyBindings);
            _trayProcessCommunicationService.UpdateToggleWindowKeyBindings();
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to restore the default keybindings?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                _settingsService.ResetKeyBindings();
                Initialize(_settingsService.GetKeyBindings());
            }
        }
    }
}