using FluentTerminal.App.ViewModels.Settings;
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

        public TerminalPageViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is TerminalPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
