using FluentTerminal.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class TerminalSettings : Page
    {
        public TerminalSettings()
        {
            InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SettingsViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
