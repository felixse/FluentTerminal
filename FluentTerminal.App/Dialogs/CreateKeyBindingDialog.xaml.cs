﻿using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Services;

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class CreateKeyBindingDialog : ContentDialog, ICreateKeyBindingDialog, INotifyPropertyChanged
    {
        private readonly IAcceleratorKeyValidator _acceleratorKeyValidator;

        private bool _ctrl;
        private bool _shift;
        private bool _alt;
        private bool _meta;
        private int _key;

        public bool Ctrl
        {
            get => _ctrl;
            set => Set(ref _ctrl, value);
        }

        public bool Shift
        {
            get => _shift;
            set => Set(ref _shift, value);
        }

        public bool Alt
        {
            get => _alt;
            set => Set(ref _alt, value);
        }

        public bool Meta
        {
            get => _meta;
            set => Set(ref _meta, value);
        }

        public int Key
        {
            get => _key;
            set
            {
                // We're ignoring invalid keys.
                if (_acceleratorKeyValidator.Valid(value))
                {
                    Set(ref _key, value);
                }
            }
        }

        public CreateKeyBindingDialog(IAcceleratorKeyValidator acceleratorKeyValidator)
        {
            _acceleratorKeyValidator = acceleratorKeyValidator;
            InitializeComponent();
            ResetCommand = new RelayCommand(Reset);
            PreviewKeyDown += RegisterKeyBindingDialog_PreviewKeyDown;
            Reset();

            this.PrimaryButtonText = I18N.Translate("OK");
            this.SecondaryButtonText = I18N.Translate("Cancel");
        }

        public RelayCommand ResetCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task<KeyBinding> CreateKeyBinding()
        {
            return ShowAsync().AsTask()
                .ContinueWith(
                    t => t.Result == ContentDialogResult.Primary
                        ? new KeyBinding {Alt = Alt, Ctrl = Ctrl, Key = Key, Meta = Meta, Shift = Shift}
                        : null, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void OnResetButtonPreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void RegisterKeyBindingDialog_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    Ctrl = true;
                    break;

                case VirtualKey.Shift:
                    Shift = true;
                    break;

                case VirtualKey.Menu:
                    Alt = true;
                    break;

                case VirtualKey.LeftWindows:
                case VirtualKey.RightWindows:
                    Meta = true;
                    break;

                default:
                    Key = (int)e.Key;
                    break;
            }

            ResetButton.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void Reset()
        {
            Alt = false;
            Ctrl = false;
            Meta = false;
            Shift = false;
            Key = 0;

            ResetButton.Visibility = Visibility.Collapsed;
        }

        private void Set<T>(ref T field, T value, [CallerMemberName]string propertyName = "")
        {
            if (field?.Equals(value) ?? value == null)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}