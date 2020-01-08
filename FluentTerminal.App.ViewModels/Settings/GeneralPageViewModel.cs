using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
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
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IFileSystemService _fileSystemService;

        public GeneralPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider,
            IStartupTaskService startupTaskService, IApplicationLanguageService applicationLanguageService,
            ITrayProcessCommunicationService trayProcessCommunicationService, IFileSystemService fileSystemService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _startupTaskService = startupTaskService;
            _applicationLanguageService = applicationLanguageService;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _fileSystemService = fileSystemService;

            _applicationSettings = _settingsService.GetApplicationSettings();

            RestoreDefaultsCommand = new AsyncCommand(RestoreDefaultsAsync);
            BrowseLogDirectoryCommand = new AsyncCommand(BrowseLogDirectoryAsync);
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

        public async Task OnNavigatedToAsync()
        {
            var startupTaskStatus = await _startupTaskService.GetStatusAsync();
            SetStartupTaskPropertiesForStatus(startupTaskStatus);
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

        public bool MuteTerminalBeeps
        {
            get => _applicationSettings.MuteTerminalBeeps;
            set
            {
                if (_applicationSettings.MuteTerminalBeeps != value)
                {
                    _applicationSettings.MuteTerminalBeeps = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();

                    _trayProcessCommunicationService.MuteTerminalAsync(value);
                }
            }
        }
        public bool EnableLogging
        {
            get => _applicationSettings.EnableLogging;
            set
            {
                if (_applicationSettings.EnableLogging != value)
                {
                    _applicationSettings.EnableLogging = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool PrintableOutputOnly
        {
            get => _applicationSettings.PrintableOutputOnly;
            set
            {
                if (_applicationSettings.PrintableOutputOnly != value)
                {
                    _applicationSettings.PrintableOutputOnly = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public string LogDirectoryPath
        {
            get => _applicationSettings.LogDirectoryPath;
            set
            {
                if (_applicationSettings.LogDirectoryPath != value)
                {
                    _applicationSettings.LogDirectoryPath = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseConPty
        {
            get => _applicationSettings.UseConPty;
            set
            {
                if (_applicationSettings.UseConPty != value)
                {
                    _applicationSettings.UseConPty = value;
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
                    RaisePropertyChanged(nameof(TabIsSelected));
                }
            }
        }

        public bool TabWindowCascadingAppMenu
        {
            get => _applicationSettings.TabWindowCascadingAppMenu;
            set
            {
                if (_applicationSettings.TabWindowCascadingAppMenu != value)
                {
                    _applicationSettings.TabWindowCascadingAppMenu = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public IAsyncCommand RestoreDefaultsCommand { get; }

        public IAsyncCommand BrowseLogDirectoryCommand { get; }

        public bool StartupTaskEnabled
        {
            get => _startupTaskEnabled;
            set
            {
                if (Set(ref _startupTaskEnabled, value))
                {
                    // ReSharper disable once AssignmentIsFullyDiscarded
                    _ = SetStartupTaskStateAsync(value);
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
            set => NewTerminalLocation = value ? NewTerminalLocation.Tab : NewTerminalLocation.Window;
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

        private async Task RestoreDefaultsAsync()
        {
            var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmRestoreGeneralSettings"), DialogButton.OK, DialogButton.Cancel);

            if (result == DialogButton.OK)
            {
                var defaults = _defaultValueProvider.GetDefaultApplicationSettings();
                ConfirmClosingWindows = defaults.ConfirmClosingWindows;
                ConfirmClosingTabs = defaults.ConfirmClosingTabs;
                UnderlineSelectedTab = defaults.UnderlineSelectedTab;
                InactiveTabColorMode = defaults.InactiveTabColorMode;
                NewTerminalLocation = defaults.NewTerminalLocation;
                ShowNewOutputIndicator = defaults.ShowNewOutputIndicator;
                EnableTrayIcon = defaults.EnableTrayIcon;
                ShowCustomTitleInTitlebar = defaults.ShowCustomTitleInTitlebar;
                UseMoshByDefault = defaults.UseMoshByDefault;
                AutoFallbackToWindowsUsernameInLinks = defaults.AutoFallbackToWindowsUsernameInLinks;
                RTrimCopiedLines = defaults.RTrimCopiedLines;
                MuteTerminalBeeps = defaults.MuteTerminalBeeps;
                EnableLogging = defaults.EnableLogging;
                PrintableOutputOnly = defaults.PrintableOutputOnly;
                LogDirectoryPath = defaults.LogDirectoryPath;
                UseConPty = defaults.UseConPty;
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

                case StartupTaskStatus.EnabledByPolicy:
                    StartupTaskEnabled = true;
                    StartupTaskErrorMessage = I18N.TranslateWithFallback("EnabledByPolicy", "Enabled by policy.");
                    CanEnableStartupTask = false;
                    break;
            }
        }

        private async Task SetStartupTaskStateAsync(bool enabled)
        {
            StartupTaskStatus status;
            if (enabled)
            {
                status = await _startupTaskService.EnableStartupTaskAsync();
            }
            else
            {
                await _startupTaskService.DisableStartupTaskAsync();
                status = await _startupTaskService.GetStatusAsync();
            }
            SetStartupTaskPropertiesForStatus(status);
        }

        private async Task BrowseLogDirectoryAsync()
        {
            var folder = await _fileSystemService.BrowseForDirectoryAsync();

            if (folder != null)
            {
                LogDirectoryPath = folder;
            }
        }
    }
}