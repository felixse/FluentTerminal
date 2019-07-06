using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class GeneralPageViewModel : ViewModelBase
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IStartupTaskService _startupTaskService;
        private bool _canEnableStartupTask;
        private bool _startupTaskEnabled;
        private bool _shouldRestartForTrayMessage;
        private string _startupTaskErrorMessage;
        private bool _needsToRestart;
        private readonly IApplicationLanguageService _applicationLanguageService;

        public GeneralPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider,
            IStartupTaskService startupTaskService, IApplicationLanguageService applicationLanguageService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _startupTaskService = startupTaskService;
            _applicationLanguageService = applicationLanguageService;

            _applicationSettings = _settingsService.GetApplicationSettings();

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
        }

        public IEnumerable<string> Languages => _applicationLanguageService.Languages;

        public bool NeedsToRestart
        {
            get => _needsToRestart;
            set => Set(ref _needsToRestart, value);
        }

        public string SelectedLanguage
        {
            get => _applicationLanguageService.GetCurrentLanguage();
            set
            {
                _applicationLanguageService.SetLanguage(value);
                NeedsToRestart = true;
            }
        }

        public async Task OnNavigatedTo()
        {
            var startupTaskStatus = await _startupTaskService.GetStatus();
            SetStartupTaskPropertiesForStatus(startupTaskStatus);
        }

        public bool AlwaysShowTabs
        {
            get => _applicationSettings.AlwaysShowTabs;
            set
            {
                if (_applicationSettings.AlwaysShowTabs != value)
                {
                    _applicationSettings.AlwaysShowTabs = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowCustomTitleInTitlebar
        {
            get => _applicationSettings.ShowCustomTitleInTitlebar;
            set
            {
                if (_applicationSettings.ShowCustomTitleInTitlebar != value)
                {
                    _applicationSettings.ShowCustomTitleInTitlebar = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool EnableTrayIcon
        {
            get => _applicationSettings.EnableTrayIcon;
            set
            {
                if (_applicationSettings.EnableTrayIcon != value)
                {
                    _applicationSettings.EnableTrayIcon = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();

                    // Toggle message telling user to restart the program
                    ShouldRestartForTrayMessage = !ShouldRestartForTrayMessage;
                }
            }
        }

        public bool ShouldRestartForTrayMessage
        {
            get => _shouldRestartForTrayMessage;
            set => Set(ref _shouldRestartForTrayMessage, value);
        }

        public bool ShowNewOutputIndicator
        {
            get => _applicationSettings.ShowNewOutputIndicator;
            set
            {
                if (_applicationSettings.ShowNewOutputIndicator != value)
                {
                    _applicationSettings.ShowNewOutputIndicator = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseMoshByDefault
        {
            get => _applicationSettings.UseMoshByDefault;
            set
            {
                if (_applicationSettings.UseMoshByDefault != value)
                {
                    _applicationSettings.UseMoshByDefault = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool AutoFallbackToWindowsUsernameInLinks
        {
            get => _applicationSettings.AutoFallbackToWindowsUsernameInLinks;
            set
            {
                if (_applicationSettings.AutoFallbackToWindowsUsernameInLinks != value)
                {
                    _applicationSettings.AutoFallbackToWindowsUsernameInLinks = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseQuickSshConnectByDefault
        {
            get => _applicationSettings.UseQuickSshConnectByDefault;
            set
            {
                if (_applicationSettings.UseQuickSshConnectByDefault != value)
                {
                    _applicationSettings.UseQuickSshConnectByDefault = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool RTrimCopiedLines
        {
            get => _applicationSettings.RTrimCopiedLines;
            set
            {
                if (_applicationSettings.RTrimCopiedLines != value)
                {
                    _applicationSettings.RTrimCopiedLines = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool BottomIsSelected
        {
            get => TabsPosition == TabsPosition.Bottom;
            set { if (value) TabsPosition = TabsPosition.Bottom; }
        }

        public bool CanEnableStartupTask
        {
            get => _canEnableStartupTask;
            set => Set(ref _canEnableStartupTask, value);
        }

        public bool ConfirmClosingTabs
        {
            get => _applicationSettings.ConfirmClosingTabs;
            set
            {
                if (_applicationSettings.ConfirmClosingTabs != value)
                {
                    _applicationSettings.ConfirmClosingTabs = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool ConfirmClosingWindows
        {
            get => _applicationSettings.ConfirmClosingWindows;
            set
            {
                if (_applicationSettings.ConfirmClosingWindows != value)
                {
                    _applicationSettings.ConfirmClosingWindows = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public NewTerminalLocation NewTerminalLocation
        {
            get => _applicationSettings.NewTerminalLocation;
            set
            {
                if (_applicationSettings.NewTerminalLocation != value)
                {
                    _applicationSettings.NewTerminalLocation = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand RestoreDefaultsCommand { get; }

        public bool StartupTaskEnabled
        {
            get => _startupTaskEnabled;
            set
            {
                if (_startupTaskEnabled != value)
                {
                    _startupTaskEnabled = value;
                    RaisePropertyChanged(nameof(StartupTaskEnabled));
                    SetStartupTaskState(value);
                }
            }
        }

        public string StartupTaskErrorMessage
        {
            get => _startupTaskErrorMessage;
            set => Set(ref _startupTaskErrorMessage, value);
        }

        public bool TabIsSelected
        {
            get => NewTerminalLocation == NewTerminalLocation.Tab;
            set { if (value) NewTerminalLocation = NewTerminalLocation.Tab; }
        }

        public TabsPosition TabsPosition
        {
            get => _applicationSettings.TabsPosition;
            set
            {
                if (_applicationSettings.TabsPosition != value)
                {
                    _applicationSettings.TabsPosition = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool TopIsSelected
        {
            get => TabsPosition == TabsPosition.Top;
            set { if (value) TabsPosition = TabsPosition.Top; }
        }

        public bool UnderlineSelectedTab
        {
            get => _applicationSettings.UnderlineSelectedTab;
            set
            {
                if (_applicationSettings.UnderlineSelectedTab != value)
                {
                    _applicationSettings.UnderlineSelectedTab = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool BackgroundIsSelected
        {
            get => InactiveTabColorMode == InactiveTabColorMode.Background;
            set { if (value) InactiveTabColorMode = InactiveTabColorMode.Background; }
        }

        public bool UnderlinedIsSelected
        {
            get => InactiveTabColorMode == InactiveTabColorMode.Underlined;
            set { if (value) InactiveTabColorMode = InactiveTabColorMode.Underlined; }
        }

        public InactiveTabColorMode InactiveTabColorMode
        {
            get => _applicationSettings.InactiveTabColorMode;
            set
            {
                if (_applicationSettings.InactiveTabColorMode != value)
                {
                    _applicationSettings.InactiveTabColorMode = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool WindowIsSelected
        {
            get => NewTerminalLocation == NewTerminalLocation.Window;
            set { if (value) NewTerminalLocation = NewTerminalLocation.Window; }
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmRestoreGeneralSettings"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                var defaults = _defaultValueProvider.GetDefaultApplicationSettings();
                ConfirmClosingWindows = defaults.ConfirmClosingWindows;
                ConfirmClosingTabs = defaults.ConfirmClosingTabs;
                UnderlineSelectedTab = defaults.UnderlineSelectedTab;
                InactiveTabColorMode = defaults.InactiveTabColorMode;
                NewTerminalLocation = defaults.NewTerminalLocation;
                AlwaysShowTabs = defaults.AlwaysShowTabs;
                ShowNewOutputIndicator = defaults.ShowNewOutputIndicator;
                EnableTrayIcon = defaults.EnableTrayIcon;
                ShowCustomTitleInTitlebar = defaults.ShowCustomTitleInTitlebar;
                UseMoshByDefault = defaults.UseMoshByDefault;
                AutoFallbackToWindowsUsernameInLinks = defaults.AutoFallbackToWindowsUsernameInLinks;
                RTrimCopiedLines = defaults.RTrimCopiedLines;
            }
        }

        private void SetStartupTaskPropertiesForStatus(StartupTaskStatus startupTaskStatus)
        {
            switch (startupTaskStatus)
            {
                case StartupTaskStatus.Enabled:
                    StartupTaskEnabled = true;
                    StartupTaskErrorMessage = string.Empty;
                    CanEnableStartupTask = true;
                    break;

                case StartupTaskStatus.Disabled:
                    StartupTaskEnabled = false;
                    StartupTaskErrorMessage = string.Empty;
                    CanEnableStartupTask = true;
                    break;

                case StartupTaskStatus.DisabledByUser:
                    StartupTaskEnabled = false;
                    StartupTaskErrorMessage = I18N.Translate("DisabledByUser");
                    CanEnableStartupTask = false;
                    break;

                case StartupTaskStatus.DisabledByPolicy:
                    StartupTaskEnabled = false;
                    StartupTaskErrorMessage = I18N.Translate("DisabledByPolicy");
                    CanEnableStartupTask = false;
                    break;
            }
        }

        private async Task SetStartupTaskState(bool enabled)
        {
            StartupTaskStatus status;
            if (enabled)
            {
                status = await _startupTaskService.EnableStartupTask();
            }
            else
            {
                await _startupTaskService.DisableStartupTask();
                status = await _startupTaskService.GetStatus();
            }
            SetStartupTaskPropertiesForStatus(status);
        }
    }
}