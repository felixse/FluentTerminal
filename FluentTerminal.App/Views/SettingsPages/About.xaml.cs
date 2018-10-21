using FluentTerminal.App.ViewModels.Settings;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class About : Page
    {
        public AboutPageViewModel ViewModel { get; private set; }
        private string baseUrl = "https://github.com/felixse/FluentTerminal/releases/tag/";

        public bool LatestVersionLoading { get => LatestVersion == null; }
        public bool LatestVersionFound { get => !LatestVersionNotFound && !LatestVersionLoading; }
        public bool LatestVersionNotFound { get => LatestVersion == "0.0.0.0"; }

        public string CurrentVersion { get => ViewModel.GetCurrentVersion(); }
        public string CurrentVersionReleaseNotesURL => baseUrl + CurrentVersion;

        public string LatestVersion { get; private set; }
        public string LatestVersionReleaseNotesURL => baseUrl + LatestVersion;

        public About()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutPageViewModel viewModel)
            {
                ViewModel = viewModel;
                GetLatestVersion();
            }
        }

        private async void GetLatestVersion()
        {
            await Task.Run(() => LatestVersion = ViewModel.GetLatestVersion());
            Bindings.Update();
        }
    }
}