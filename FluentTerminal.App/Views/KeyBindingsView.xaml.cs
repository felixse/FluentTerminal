using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class KeyBindingsView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(KeyBindingsViewModel), typeof(KeyBindingsView), new PropertyMetadata(null));

        public KeyBindingsView()
        {
            InitializeComponent();
        }

        public KeyBindingsViewModel ViewModel
        {
            get { return (KeyBindingsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}