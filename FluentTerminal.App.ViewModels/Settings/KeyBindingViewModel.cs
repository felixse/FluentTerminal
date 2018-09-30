using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;

        public KeyBindingViewModel(KeyBinding keyBinding, IDialogService dialogService)
        {
            Model = keyBinding;
            _dialogService = dialogService;
            EditCommand = new RelayCommand(async () => await Edit().ConfigureAwait(false));
            DeleteCommand = new RelayCommand(async () => await Delete().ConfigureAwait(false));
        }

        public bool Editable { get; set; } = true;

        public event EventHandler Deleted;

        public event EventHandler Edited;

        public bool Meta
        {
            get => Model.Meta;
            set
            {
                if (Model.Meta != value)
                {
                    Model.Meta = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }

        public int Key
        {
            get => Model.Key;
            set
            {
                if (Model.Key != value)
                {
                    Model.Key = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                }
            }
        }

        public async Task<bool> Edit()
        {
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

        private async Task Delete()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to delete this keybinding?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}