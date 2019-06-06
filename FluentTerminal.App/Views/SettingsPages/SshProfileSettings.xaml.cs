using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class SshProfileSettings : Page
    {
        public SshProfilesPageViewModel ViewModel { get; private set; }

        public SshProfileSettings()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SshProfilesPageViewModel viewModel)
            {
                ViewModel = viewModel;
                if (ViewModel.SshProfiles.Count == 0)
                {
                    ViewModel.CreateSshProfile();
                }
            }
        }
    }
}
