using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class KeyBindingSettings : Page
    {
        public KeyBindingsPageViewModel ViewModel { get; private set; }

        public KeyBindingSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is KeyBindingsPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
