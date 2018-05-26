using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class ShellProfileSettings : Page
    {
        public ProfilesPageViewModel ViewModel { get; private set; }

        public ShellProfileSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ProfilesPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
