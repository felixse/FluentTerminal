using FluentTerminal.App.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class AboutPageViewModel : ObservableObject
    {
        private const string BaseUrl = "https://github.com/felixse/FluentTerminal/releases/tag/";
        private readonly IUpdateService _updateService;
        private string _latestVersion;

        public AboutPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;

            CheckForUpdatesCommand = new AsyncRelayCommand(() => CheckForUpdateAsync(true));
        }

        public ICommand CheckForUpdatesCommand { get; }

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
                if (SetProperty(ref _latestVersion, value))
                {
                    OnPropertyChanged(nameof(LatestVersionFound));
                    OnPropertyChanged(nameof(LatestVersionLoading));
                    OnPropertyChanged(nameof(LatestVersionNotFound));
                    OnPropertyChanged(nameof(LatestVersionReleaseNotesURL));
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
            //await _applicationView.ExecuteOnUiThreadAsync(() =>
            //        LatestVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}")
            //    .ConfigureAwait(false);
            await _updateService.CheckForUpdateAsync(notifyNoUpdate).ConfigureAwait(false);
        }
    }
}