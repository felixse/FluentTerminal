using FluentTerminal.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class ThemeSettings : Page
    {
        public SettingsViewModel ViewModel { get; private set; }

        public ThemeSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SettingsViewModel viewModel)
            {
                ViewModel = viewModel;
            }
            TitleBar.RestoreColors();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            TitleBar.ResetTitleBar();
        }

    }
}
