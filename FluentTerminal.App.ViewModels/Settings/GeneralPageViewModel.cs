using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        private bool _editingNewTerminalLocation;
        private bool _editingTabsPosition;
        private bool _editingInactiveTabColorMode;
        private bool _startupTaskEnabled;
        private string _startupTaskErrorMessage;

        public GeneralPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider, IStartupTaskService startupTaskService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _startupTaskService = startupTaskService;

            _applicationSettings = _settingsService.GetApplicationSettings();

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
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

        public bool BottomIsSelected
        {
            get => TabsPosition == TabsPosition.Bottom;
            set => TabsPosition = TabsPosition.Bottom;
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
                if (_applicationSettings.NewTerminalLocation != value && !_editingNewTerminalLocation)
                {
                    _editingNewTerminalLocation = true;
                    _applicationSettings.NewTerminalLocation = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(WindowIsSelected));
                    RaisePropertyChanged(nameof(TabIsSelected));
                    _editingNewTerminalLocation = false;
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
            set => NewTerminalLocation = NewTerminalLocation.Tab;
        }

        public TabsPosition TabsPosition
        {
            get => _applicationSettings.TabsPosition;
            set
            {
                if (_applicationSettings.TabsPosition != value && !_editingTabsPosition)
                {
                    _editingTabsPosition = true;
                    _applicationSettings.TabsPosition = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(TopIsSelected));
                    RaisePropertyChanged(nameof(BottomIsSelected));
                    _editingTabsPosition = false;
                }
            }
        }

        public bool TopIsSelected
        {
            get => TabsPosition == TabsPosition.Top;
            set => TabsPosition = TabsPosition.Top;
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
            set => InactiveTabColorMode = InactiveTabColorMode.Background;
        }

        public bool UnderlinedIsSelected
        {
            get => InactiveTabColorMode == InactiveTabColorMode.Underlined;
            set => InactiveTabColorMode = InactiveTabColorMode.Underlined;
        }

        public InactiveTabColorMode InactiveTabColorMode
        {
            get => _applicationSettings.InactiveTabColorMode;
            set
            {
                if (_applicationSettings.InactiveTabColorMode != value && !_editingInactiveTabColorMode)
                {
                    _editingInactiveTabColorMode = true;
                    _applicationSettings.InactiveTabColorMode = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(BackgroundIsSelected));
                    RaisePropertyChanged(nameof(UnderlinedIsSelected));
                    _editingInactiveTabColorMode = false;
                }
            }
        }

        public bool WindowIsSelected
        {
            get => NewTerminalLocation == NewTerminalLocation.Window;
            set => NewTerminalLocation = NewTerminalLocation.Window;
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to restore the general settings?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

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
                    StartupTaskErrorMessage = "Disabled by user. Please reactivate it in the Startup tab of the Task Manager.";
                    CanEnableStartupTask = false;
                    break;

                case StartupTaskStatus.DisabledByPolicy:
                    StartupTaskEnabled = false;
                    StartupTaskErrorMessage = "Disabled by policy.";
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
                _startupTaskService.DisableStartupTask();
                status = await _startupTaskService.GetStatus();
            }
            SetStartupTaskPropertiesForStatus(status);
        }
    }
}