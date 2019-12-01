using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using GalaSoft.MvvmLight;
using System;
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

            CheckForUpdatesCommand = new AsyncCommand(() => CheckForUpdate(true));
        }

        public IAsyncCommand CheckForUpdatesCommand { get; }

        public string CurrentVersion
        {
            get
            {
                var version = _updateService.GetCurrentVersion();
                return ConvertVersionToString(version);
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
            return CheckForUpdate(false);
        }

        public async Task CheckForUpdate(bool notifyNoUpdate)
        {
            var version = ConvertVersionToString(await _updateService.GetLatestVersionAsync());
            await _applicationView.ExecuteOnUiThreadAsync(() => LatestVersion = version);
            await _updateService.CheckForUpdate(notifyNoUpdate);
        }

        private string ConvertVersionToString(Version version)
        {
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}