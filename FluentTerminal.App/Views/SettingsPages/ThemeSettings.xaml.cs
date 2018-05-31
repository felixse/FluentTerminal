using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class ThemeSettings : Page
    {
        public ThemesPageViewModel ViewModel { get; private set; }

        public ThemeSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ThemesPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
