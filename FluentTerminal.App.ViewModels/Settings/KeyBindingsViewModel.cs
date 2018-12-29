using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingsViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ICollection<KeyBinding> _keyBindings;
        private bool _editable;

        public KeyBindingsViewModel(string command, ICollection<KeyBinding> keyBindings, IDialogService dialogService, string commandName, bool editable)
        {
            Command = command;
            _keyBindings = keyBindings;
            _dialogService = dialogService;

            CommandName = commandName;
            _editable = editable;

            foreach (var binding in keyBindings)
            {
                var viewModel = new KeyBindingViewModel(binding, _dialogService, this);
                viewModel.Deleted += ViewModel_Deleted;
                viewModel.Edited += ViewModel_Edited;
                KeyBindings.Add(viewModel);
            }
        }

        public delegate void EditedEvent(string command, ICollection<KeyBinding> keyBindings);

        public event EditedEvent Edited;

        public string Command { get; }

        public string CommandName { get; }

        /// <summary>
        ///  Whether or not the "Edit" and "Delete" links appear next to the key binding list.
        /// </summary>
        public bool Editable
        {
            get => _editable;
            set => Set(ref _editable, value);
        }

        public ObservableCollection<KeyBindingViewModel> KeyBindings { get; } = new ObservableCollection<KeyBindingViewModel>();

        public async Task ShowAddKeyBindingDialog()
        {
            var newKeyBinding = new KeyBindingViewModel(new KeyBinding { Command = Command }, _dialogService, this);

            if (await newKeyBinding.Edit().ConfigureAwait(true))
            {
                Add(newKeyBinding.Model);
            }
        }

        public void Add(KeyBinding keyBinding)
        {
            var viewModel = new KeyBindingViewModel(keyBinding, _dialogService, this);

            viewModel.Deleted += ViewModel_Deleted;
            viewModel.Edited += ViewModel_Edited;
            KeyBindings.Add(viewModel);
            _keyBindings.Add(viewModel.Model);
            Edited?.Invoke(Command, _keyBindings);
        }

        private void ViewModel_Deleted(object sender, EventArgs e)
        {
            if (sender is KeyBindingViewModel keyBinding)
            {
                _keyBindings.Remove(keyBinding.Model);
                KeyBindings.Remove(keyBinding);
                Edited?.Invoke(Command, _keyBindings);
            }
        }

        private void ViewModel_Edited(object sender, EventArgs e)
        {
            Edited?.Invoke(Command, _keyBindings);
        }
    }
}