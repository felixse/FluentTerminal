using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class About : Page
    {
        public AboutPageViewModel ViewModel { get; private set; }

        public About()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}