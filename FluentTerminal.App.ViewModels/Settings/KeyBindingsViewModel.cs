using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingsViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private bool _editable;

        public KeyBindingsViewModel(string command, IDialogService dialogService, string commandName, bool editable)
        {
            _dialogService = dialogService;

            Command = command;
            CommandName = commandName;
            Editable = editable;
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

        // Requires UI thread
        public async Task ShowAddKeyBindingDialogAsync()
        {
            var newKeyBinding = new KeyBindingViewModel(new KeyBinding { Command = Command }, _dialogService, this);

            // ConfigureAwait(true) because we need to execute Add method in the calling (UI) thread.
            if (await newKeyBinding.EditAsync().ConfigureAwait(true))
            {
                Add(newKeyBinding.Model);
            }
        }

        // Requires UI thread
        public void Clear()
        {
            KeyBindings.Clear();
        }

        // Requires UI thread
        public void Add(KeyBinding keyBinding)
        {
            var viewModel = new KeyBindingViewModel(keyBinding, _dialogService, this);

            viewModel.Deleted += ViewModel_Deleted;
            viewModel.Edited += ViewModel_Edited;
            KeyBindings.Add(viewModel);
            Edited?.Invoke(Command, KeyBindings.Select(x => x.Model).ToList());
        }

        private void ViewModel_Deleted(object sender, EventArgs e)
        {
            if (sender is KeyBindingViewModel keyBinding)
            {
                KeyBindings.Remove(keyBinding);
                Edited?.Invoke(Command, KeyBindings.Select(x => x.Model).ToList());
            }
        }

        private void ViewModel_Edited(object sender, EventArgs e)
        {
            Edited?.Invoke(Command, KeyBindings.Select(x => x.Model).ToList());
        }
    }
}