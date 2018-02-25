using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingsViewModel : ViewModelBase
    {
        private readonly Command _command;
        private readonly IDialogService _dialogService;
        private ICollection<KeyBinding> _keyBindings;

        public KeyBindingsViewModel(Command command, ICollection<KeyBinding> keyBindings, IDialogService dialogService)
        {
            _command = command;
            _keyBindings = keyBindings;
            _dialogService = dialogService;
            AddCommand = new RelayCommand(async () => await Add());

            foreach (var binding in keyBindings)
            {
                var viewModel = new KeyBindingViewModel(binding, _dialogService);
                viewModel.Deleted += ViewModel_Deleted;
                viewModel.Edited += ViewModel_Edited;
                KeyBindings.Add(viewModel);
            }
        }

        public event EventHandler Edited;

        public RelayCommand AddCommand { get; }

        public ObservableCollection<KeyBindingViewModel> KeyBindings { get; } = new ObservableCollection<KeyBindingViewModel>();

        private async Task Add()
        {
            var newKeyBinding = new KeyBindingViewModel(new KeyBinding { Command = _command }, _dialogService);

            if (await newKeyBinding.Edit())
            {
                newKeyBinding.Deleted += ViewModel_Deleted;
                newKeyBinding.Edited += ViewModel_Edited;
                KeyBindings.Add(newKeyBinding);
                _keyBindings.Add(newKeyBinding.Model);
                Edited?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ViewModel_Deleted(object sender, EventArgs e)
        {
            if (sender is KeyBindingViewModel keyBinding)
            {
                _keyBindings.Remove(keyBinding.Model);
                KeyBindings.Remove(keyBinding);
                Edited?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ViewModel_Edited(object sender, EventArgs e)
        {
            Edited?.Invoke(this, EventArgs.Empty);
        }
    }
}