using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class ShellPageViewModel : ViewModelBase
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly ShellConfiguration _shellConfiguration;
        private bool _editingShellType;

        public ShellPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
            BrowseForCustomShellCommand = new RelayCommand(async () => await BrowseForCustomShell().ConfigureAwait(false));
            BrowseForWorkingDirectoryCommand = new RelayCommand(async () => await BrowseForWorkingDirectory().ConfigureAwait(false));

            _shellConfiguration = _settingsService.GetShellConfiguration();
        }

        public string Arguments
        {
            get => _shellConfiguration.Arguments;
            set
            {
                if (_shellConfiguration.Arguments != value)
                {
                    _shellConfiguration.Arguments = value;
                    _settingsService.SaveShellConfiguration(_shellConfiguration);
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand BrowseForCustomShellCommand { get; }

        public RelayCommand BrowseForWorkingDirectoryCommand { get; }

        public bool CMDIsSelected
        {
            get => ShellType == ShellType.CMD;
            set => ShellType = ShellType.CMD;
        }

        public bool CustomShellIsSelected
        {
            get => ShellType == ShellType.Custom;
            set => ShellType = ShellType.Custom;
        }

        public string CustomShellLocation
        {
            get => _shellConfiguration.CustomShellLocation;
            set
            {
                if (_shellConfiguration.CustomShellLocation != value)
                {
                    _shellConfiguration.CustomShellLocation = value;
                    _settingsService.SaveShellConfiguration(_shellConfiguration);
                    RaisePropertyChanged();
                }
            }
        }

        public bool PoweShellIsSelected
        {
            get => ShellType == ShellType.PowerShell;
            set => ShellType = ShellType.PowerShell;
        }

        public RelayCommand RestoreDefaultsCommand { get; }

        public ShellType ShellType
        {
            get => _shellConfiguration.Shell;
            set
            {
                if (_shellConfiguration.Shell != value && !_editingShellType)
                {
                    _editingShellType = true;
                    _shellConfiguration.Shell = value;
                    _settingsService.SaveShellConfiguration(_shellConfiguration);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CustomShellIsSelected));
                    RaisePropertyChanged(nameof(PoweShellIsSelected));
                    RaisePropertyChanged(nameof(CMDIsSelected));
                    _editingShellType = false;
                }
            }
        }

        public string WorkingDirectory
        {
            get => _shellConfiguration.WorkingDirectory;
            set
            {
                if (_shellConfiguration.WorkingDirectory != value)
                {
                    _shellConfiguration.WorkingDirectory = value;
                    _settingsService.SaveShellConfiguration(_shellConfiguration);
                    RaisePropertyChanged();
                }
            }
        }

        private async Task BrowseForCustomShell()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".exe");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                CustomShellLocation = file.Path;
            }
        }

        private async Task BrowseForWorkingDirectory()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add(".whatever"); // else a ComException is thrown
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                WorkingDirectory = folder.Path;
            }
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowDialogAsnyc("Please confirm", "Are you sure you want to restore the default shell configuration?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(false);

            if (result == DialogButton.OK)
            {
                var defaults = _defaultValueProvider.GetDefaultShellConfiguration();
                ShellType = defaults.Shell;
                CustomShellLocation = defaults.CustomShellLocation;
                WorkingDirectory = defaults.WorkingDirectory;
                Arguments = defaults.Arguments;
            }
        }
    }
}