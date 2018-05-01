using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
using GalaSoft.MvvmLight.Command;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class CreateKeyBindingDialog : ContentDialog
    {
        public static readonly DependencyProperty KeyBindingProperty =
            DependencyProperty.Register(nameof(KeyBinding), typeof(KeyBindingViewModel), typeof(CreateKeyBindingDialog), new PropertyMetadata(null));

        private readonly IDialogService _dialogService;

        public CreateKeyBindingDialog(IDialogService dialogService)
        {
            _dialogService = dialogService;
            InitializeComponent();
            ResetCommand = new RelayCommand(Reset);
            PreviewKeyDown += RegisterKeyBindingDialog_PreviewKeyDown;
            Reset();
        }

        public KeyBindingViewModel KeyBinding
        {
            get { return (KeyBindingViewModel)GetValue(KeyBindingProperty); }
            set { SetValue(KeyBindingProperty, value); }
        }

        public RelayCommand ResetCommand { get; }

        private void OnResetButtonPreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void RegisterKeyBindingDialog_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    KeyBinding.Ctrl = true;
                    break;
                case VirtualKey.Shift:
                    KeyBinding.Shift = true;
                    break;
                case VirtualKey.Menu:
                    KeyBinding.Alt = true;
                    break;
                case VirtualKey.LeftWindows:
                case VirtualKey.RightWindows:
                    KeyBinding.Meta = true;
                    break;
                default:
                    KeyBinding.Key = (int)e.Key;
                    break;
            }

            ResetButton.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void Reset()
        {
            KeyBinding = new KeyBindingViewModel(new KeyBinding(), _dialogService);
            ResetButton.Visibility = Visibility.Collapsed;
        }
    }
}