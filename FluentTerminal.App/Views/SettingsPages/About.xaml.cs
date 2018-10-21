using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class About : Page
    {
        public AboutPageViewModel ViewModel { get; private set; }

        public string LatestVersion { get; private set; }
        public string LatestVersionReleaseNotesURL => "https://github.com/felixse/FluentTerminal/releases/tag/" + LatestVersion;
        public bool LatestVersionAvailable { get => LatestVersion != "loading..."; }

        public About()
        {
            InitializeComponent();
            LatestVersion = "loading...";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutPageViewModel viewModel)
            {
                ViewModel = viewModel;
                test();
            }
        }

        private async void test()
        {
            var version = await ViewModel._updateService.GetLatestVersion();
            LatestVersion = ViewModel.ConvertVersionToString(version);
            Bindings.Update();
        }
    }
}