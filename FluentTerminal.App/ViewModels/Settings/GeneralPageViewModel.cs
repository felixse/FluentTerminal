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
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly ApplicationSettings _applicationSettings;
        private bool _editingNewTerminalLocation;

        public GeneralPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _applicationSettings = _settingsService.GetApplicationSettings();

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
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

        public bool TabIsSelected
        {
            get => NewTerminalLocation == NewTerminalLocation.Tab;
            set => NewTerminalLocation = NewTerminalLocation.Tab;
        }

        public bool WindowIsSelected
        {
            get => NewTerminalLocation == NewTerminalLocation.Window;
            set => NewTerminalLocation = NewTerminalLocation.Window;
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowDialogAsnyc("Please confirm", "Are you sure you want to restore the general settings?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(false);

            if (result == DialogButton.OK)
            {
                var defaults = _defaultValueProvider.GetDefaultApplicationSettings();
                ConfirmClosingWindows = defaults.ConfirmClosingWindows;
                ConfirmClosingTabs = defaults.ConfirmClosingTabs;
                NewTerminalLocation = defaults.NewTerminalLocation;
            }
        }
    }
}