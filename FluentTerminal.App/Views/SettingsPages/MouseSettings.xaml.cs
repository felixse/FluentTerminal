using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class MouseSettings : Page
    {
        public MouseSettings()
        {
            this.InitializeComponent();
        }

        public MousePageViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MousePageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }
    }
}
