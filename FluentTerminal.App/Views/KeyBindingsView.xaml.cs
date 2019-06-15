using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class KeyBindingsView : UserControl
    {
        public static readonly DependencyProperty ShowCommandNameProperty =
            DependencyProperty.Register(nameof(ShowCommandName), typeof(bool), typeof(KeyBindingsView), new PropertyMetadata(true));

        public static readonly DependencyProperty ViewModelProperty =
                            DependencyProperty.Register(nameof(ViewModel), typeof(KeyBindingsViewModel), typeof(KeyBindingsView), new PropertyMetadata(null));

        public KeyBindingsView()
        {
            InitializeComponent();
        }

        public bool ShowCommandName
        {
            get { return (bool)GetValue(ShowCommandNameProperty); }
            set { SetValue(ShowCommandNameProperty, value); }
        }

        public KeyBindingsViewModel ViewModel
        {
            get { return (KeyBindingsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}