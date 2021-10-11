using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        public KeyBindingViewModel(KeyBinding keyBinding, IDialogService dialogService, KeyBindingsViewModel parent)
        {
            Model = keyBinding;
            Parent = parent;
            _dialogService = dialogService;
            EditCommand = new AsyncRelayCommand(EditAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        }

        // Needs to be triggered from the UI thread
        public event EventHandler Deleted;

        public event EventHandler Edited;

        public KeyBindingsViewModel Parent { get; }

        public bool Meta
        {
            get => Model.Meta;
            set
            {
                if (Model.Meta != value)
                {
                    Model.Meta = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Alt
        {
            get => Model.Alt;
            set
            {
                if (Model.Alt != value)
                {
                    Model.Alt = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Ctrl
        {
            get => Model.Ctrl;
            set
            {
                if (Model.Ctrl != value)
                {
                    Model.Ctrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }

        public int Key
        {
            get => Model.Key;
            set
            {
                if (Model.Key != value)
                {
                    Model.Key = value;
                    OnPropertyChanged();
                }
            }
        }

        public KeyBinding Model { get; }

        public bool Shift
        {
            get => Model.Shift;
            set
            {
                if (Model.Shift != value)
                {
                    Model.Shift = value;
                    OnPropertyChanged();
                }
            }
        }

        // Requires UI thread
        public async Task<bool> EditAsync()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var keyBinding = await _dialogService.ShowCreateKeyBindingDialog().ConfigureAwait(true);

            if (keyBinding != null)
            {
                Alt = keyBinding.Alt;
                Ctrl = keyBinding.Ctrl;
                Shift = keyBinding.Shift;
                Meta = keyBinding.Meta;
                Key = keyBinding.Key;

                Edited?.Invoke(this, EventArgs.Empty);

                return true;
            }

            return false;
        }

        // Requires UI thread
        private async Task DeleteAsync()
        {
            // ConfigureAwait(true) because we need to trigger Deleted event in the calling (UI) thread.
            var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmDeleteKeybindings"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}