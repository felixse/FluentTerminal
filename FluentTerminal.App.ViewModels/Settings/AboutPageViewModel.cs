using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Implementation;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class AboutPageViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;

        public AboutPageViewModel(ISettingsService settingsService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
        }

        public string CurrentVersion
        {
            get { return TrayProcessCommunicationService.GetAppVersion(); }
        }

        public string CurrentVersionReleaseNotesURL
        {
            get { return "https://github.com/felixse/FluentTerminal/releases/tag/" + CurrentVersion; }
        }

        public string LatestVersion
        {
            get { return TrayProcessCommunicationService.GetAppVersion(); }
        }

        public string LatestVersionReleaseNotesURL
        {
            get { return "https://github.com/felixse/FluentTerminal/releases/tag/" + LatestVersion; }
        }
    }
}