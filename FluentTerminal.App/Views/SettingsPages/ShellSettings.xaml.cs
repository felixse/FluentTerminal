using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class ShellSettings : Page
    {
        public ShellPageViewModel ViewModel { get; private set; }

        public ShellSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ShellPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
