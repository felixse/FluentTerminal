using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
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
        private IDictionary<string, ICollection<KeyBinding>> _keyBindings;

        public KeyBindingsPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider, ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
            AddCommand = new RelayCommand<string>(async command => await Add(command).ConfigureAwait(false));

            Initialize(_settingsService.GetCommandKeyBindings());
        }

        public RelayCommand<string> AddCommand { get; }
        public ObservableCollection<KeyBindingsViewModel> KeyBindings { get; } = new ObservableCollection<KeyBindingsViewModel>();
        public RelayCommand RestoreDefaultsCommand { get; }

        private async Task Add(string command)
        {
            var keyBinding = KeyBindings.FirstOrDefault(k => k.Command == command);
            await keyBinding?.ShowAddKeyBindingDialog();
        }

        private void ClearKeyBindings()
        {
            foreach (var keyBinding in KeyBindings)
            {
                keyBinding.Edited -= OnEdited;
            }

            KeyBindings.Clear();
        }

        private void Initialize(IDictionary<string, ICollection<KeyBinding>> keyBindings)
        {
            _keyBindings = keyBindings;

            ClearKeyBindings();

            foreach (var value in Enum.GetValues(typeof(Command)))
            {
                var command = (Command)value;
                var viewModel = new KeyBindingsViewModel(command.ToString(), _dialogService, I18N.Translate($"{nameof(Command)}.{command}"), true);
                foreach (var keyBinding in _keyBindings[command.ToString()])
                {
                    viewModel.Add(keyBinding);
                }
                viewModel.Edited += OnEdited;
                KeyBindings.Add(viewModel);
            }
        }

        private void OnEdited(string command, ICollection<KeyBinding> keyBindings)
        {
            _settingsService.SaveKeyBindings(command, keyBindings);
            _trayProcessCommunicationService.UpdateToggleWindowKeyBindings();
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmRestoreKeybindings"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                _settingsService.ResetKeyBindings();
                Initialize(_settingsService.GetCommandKeyBindings());
            }
        }
    }
}