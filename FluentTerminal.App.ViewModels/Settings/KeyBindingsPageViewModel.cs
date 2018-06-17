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

        public IEnumerable<ShellProfile> ShellProfiles { get { return _settingsService.GetShellProfiles(); } }

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

        public void UpdateKeyBindings()
        {
            Initialize(_settingsService.GetKeyBindings());
        }

        private void Initialize(IDictionary<Command, ICollection<KeyBinding>> keyBindings)
        {
            _keyBindings = keyBindings;

            ClearKeyBindings();

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                // Don't enumerate explicit keybinding enums that are in the range of a profile shortcut
                // since they won't be directly assigned to.
                if (command < Command.ShellProfileShortcut)
                {
                    var viewModel = new KeyBindingsViewModel(command, _keyBindings[command], _dialogService);
                    viewModel.Edited += OnEdited;
                    KeyBindings.Add(viewModel);
                }
            }

            foreach (ShellProfile shellProfile in _settingsService.GetShellProfiles())
            {
                ICollection<KeyBinding> shellKeyBindings = shellProfile.KeyBinding;
                if (shellKeyBindings != null)
                {
                    var viewModel = new KeyBindingsViewModel(shellProfile.KeyBindingCommand, shellKeyBindings, _dialogService, shellProfile.Name + " Shortcut");
                    viewModel.Edited += OnEdited;
                    KeyBindings.Add(viewModel);
                }
            }
        }

        private void OnEdited(Command command, ICollection<KeyBinding> keyBindings)
        {
            if (command < Command.ShellProfileShortcut)
            {
                _settingsService.SaveKeyBindings(command, keyBindings);
                _trayProcessCommunicationService.UpdateToggleWindowKeyBindings();
            }
            else
            {
                ShellProfile shellProfile = null;
                // If the command is a shell profile keybinding, find that shell and save it.
                foreach (ShellProfile _shellProfile in _settingsService.GetShellProfiles())
                {
                    if (_shellProfile.KeyBindingCommand == command)
                    {
                        shellProfile = _shellProfile;
                        break;
                    }
                }

                if (shellProfile != null)
                {
                    shellProfile.KeyBinding = keyBindings;
                    _settingsService.SaveShellProfile(shellProfile, true);
                }
            }
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