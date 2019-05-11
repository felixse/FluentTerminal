using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class KeyBindingViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;

        public KeyBindingViewModel(KeyBinding keyBinding, IDialogService dialogService, KeyBindingsViewModel parent)
        {
            Model = keyBinding;
            Parent = parent;
            _dialogService = dialogService;
            EditCommand = new AsyncCommand(Edit);
            DeleteCommand = new AsyncCommand(Delete);
        }

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

        public IAsyncCommand DeleteCommand { get; }
        public IAsyncCommand EditCommand { get; }

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
            var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDeleteKeybindings"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}