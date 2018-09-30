using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
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
        private bool _editable = true;

        public KeyBindingsViewModel(AbstractCommand command, ICollection<KeyBinding> keyBindings, IDialogService dialogService, string commandNameOverride = null)
        {
            Command = command;
            _keyBindings = keyBindings;
            _dialogService = dialogService;

            CommandName = commandNameOverride ?? command.Description;

            foreach (var binding in keyBindings)
            {
                var viewModel = new KeyBindingViewModel(binding, _dialogService);
                viewModel.Deleted += ViewModel_Deleted;
                viewModel.Edited += ViewModel_Edited;
                KeyBindings.Add(viewModel);
            }
        }

        public delegate void EditedEvent(AbstractCommand command, ICollection<KeyBinding> keyBindings);

        public event EditedEvent Edited;

        public AbstractCommand Command { get; }

        public string CommandName { get; }

        /// <summary>
        ///  Whether or not the "Edit" and "Delete" links appear next to the key binding list.
        /// </summary>
        public bool Editable
        {
            get { return _editable; }
            set
            {
                _editable = value;
                foreach (KeyBindingViewModel viewModel in KeyBindings)
                {
                    viewModel.Editable = value;
                }
            }
        }

        public ObservableCollection<KeyBindingViewModel> KeyBindings { get; } = new ObservableCollection<KeyBindingViewModel>();

        public async Task Add()
        {
            var newKeyBinding = new KeyBindingViewModel(new KeyBinding { }, _dialogService);

            if (await newKeyBinding.Edit().ConfigureAwait(true))
            {
                newKeyBinding.Deleted += ViewModel_Deleted;
                newKeyBinding.Edited += ViewModel_Edited;
                KeyBindings.Add(newKeyBinding);
                _keyBindings.Add(newKeyBinding.Model);
                Edited?.Invoke(Command, _keyBindings);
            }
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