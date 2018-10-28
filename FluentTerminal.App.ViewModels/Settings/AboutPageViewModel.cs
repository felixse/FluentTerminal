using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class AboutPageViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;
        private readonly string baseUrl = "https://github.com/felixse/FluentTerminal/releases/tag/";

        private string _latestVersion;

        public RelayCommand CheckForUpdatesCommand { get; }

        public bool LatestVersionLoading { get => LatestVersion == null; }
        public bool LatestVersionFound { get => !LatestVersionNotFound && !LatestVersionLoading; }
        public bool LatestVersionNotFound { get => LatestVersion == "0.0.0.0"; }

        public string CurrentVersion
        {
            get {
                var version = _updateService.GetCurrentVersion();
                return ConvertVersionToString(version);
            }
        }
        public string CurrentVersionReleaseNotesURL => baseUrl + CurrentVersion;

        public string LatestVersion
        {
            get => _latestVersion;
            set => Set(ref _latestVersion, value);
        }
        public string LatestVersionReleaseNotesURL => baseUrl + LatestVersion;

        public AboutPageViewModel(ISettingsService settingsService, IUpdateService updateService)
        {
            _settingsService = settingsService;
            _updateService = updateService;

            CheckForUpdatesCommand = new RelayCommand(() => _updateService.CheckForUpdate(true));

            Task.Run(() => {
                var version = _updateService.GetLatestVersion();
                LatestVersion = ConvertVersionToString(version);
            });
        }

        private string ConvertVersionToString(Version version)
        {
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}