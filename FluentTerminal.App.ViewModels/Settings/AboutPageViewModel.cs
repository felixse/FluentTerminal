using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class AboutPageViewModel : ViewModelBase
    {
        private const string BaseUrl = "https://github.com/felixse/FluentTerminal/releases/tag/";
        private readonly IUpdateService _updateService;
        private string _latestVersion;
        private readonly IApplicationView _applicationView;

        public AboutPageViewModel(IUpdateService updateService, IApplicationView applicationView)
        {
            _updateService = updateService;
            _applicationView = applicationView;

            CheckForUpdatesCommand = new AsyncCommand(() => CheckForUpdateAsync(true));
        }

        public IAsyncCommand CheckForUpdatesCommand { get; }

        public string CurrentVersion
        {
            get
            {
                var version = _updateService.GetCurrentVersion();
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        public string CurrentVersionReleaseNotesURL => BaseUrl + CurrentVersion;

        public string LatestVersion
        {
            get => _latestVersion;
            set
            {
                if (Set(ref _latestVersion, value))
                {
                    RaisePropertyChanged(nameof(LatestVersionFound));
                    RaisePropertyChanged(nameof(LatestVersionLoading));
                    RaisePropertyChanged(nameof(LatestVersionNotFound));
                    RaisePropertyChanged(nameof(LatestVersionReleaseNotesURL));
                }
            }
        }

        public bool LatestVersionFound => !LatestVersionNotFound && !LatestVersionLoading;

        public bool LatestVersionLoading => LatestVersion == null;

        public bool LatestVersionNotFound => LatestVersion == "0.0.0.0";

        public string LatestVersionReleaseNotesURL => BaseUrl + LatestVersion;

        public Task OnNavigatedTo()
        {
            return CheckForUpdateAsync(false);
        }

        private async Task CheckForUpdateAsync(bool notifyNoUpdate)
        {
            var version = await _updateService.GetLatestVersionAsync().ConfigureAwait(false);
            await _applicationView.ExecuteOnUiThreadAsync(() =>
                    LatestVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}")
                .ConfigureAwait(false);
            await _updateService.CheckForUpdateAsync(notifyNoUpdate).ConfigureAwait(false);
        }
    }
}