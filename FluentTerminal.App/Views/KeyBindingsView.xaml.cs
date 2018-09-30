using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class KeyBindingsView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
                    DependencyProperty.Register(nameof(ViewModel), typeof(KeyBindingsViewModel), typeof(KeyBindingsView), new PropertyMetadata(null));

        private bool _editable = true;

        public KeyBindingsView()
        {
            InitializeComponent();
        }

        public bool Editable
        {
            get { if (ViewModel != null) { return ViewModel.Editable; } else { return _editable; } }
            set { if (ViewModel != null) { ViewModel.Editable = value; } else { _editable = value; } }
        }

        public KeyBindingsViewModel ViewModel
        {
            get { return (KeyBindingsViewModel)GetValue(ViewModelProperty); }
            set { value.Editable = _editable; SetValue(ViewModelProperty, value); }
        }
    }
}