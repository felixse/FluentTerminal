using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace FluentTerminal.App.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private ShellConfiguration _shellConfiguration;
        private TerminalOptions _terminalOptions;
        private ThemeViewModel _selectedTheme;

        public SettingsViewModel(ISettingsService settingsService, IDefaultValueProvider defaultValueProvider, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _defaultValueProvider = defaultValueProvider;
            _dialogService = dialogService;
            BrowseForCustomShellCommand = new RelayCommand(async () => await BrowseForCustomShell());
            BrowseForWorkingDirectoryCommand = new RelayCommand(async () => await BrowseForWorkingDirectory());
            RestoreDefaultsCommand = new RelayCommand<string>(async (area) => await RestoreDefaults(area));
            CreateThemeCommand = new RelayCommand(CreateTheme);

            _shellConfiguration = _settingsService.GetShellConfiguration();
            _terminalOptions = _settingsService.GetTerminalOptions();
            ShellType = _shellConfiguration.Shell;

            var activeThemeId = _settingsService.GetCurrentThemeId();
            foreach (var theme in _settingsService.GetThemes())
            {
                var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService);
                viewModel.Activated += OnThemeActivated;
                viewModel.Deleted += OnThemeDeleted;

                if (theme.Id == activeThemeId)
                {
                    viewModel.IsActive = true;
                }
                Themes.Add(viewModel);
            }

            SelectedTheme = Themes.First();

            Fonts = CanvasTextFormat.GetSystemFontFamilies().OrderBy(s => s);

            Sizes = Enumerable.Range(1, 72);
        }

        private void OnThemeActivated(object sender, EventArgs e)
        {
            if (sender is ThemeViewModel activatedTheme)
            {
                _settingsService.SaveCurrentThemeId(activatedTheme.Id);

                foreach (var theme in Themes)
                {
                    theme.IsActive = theme.Id == activatedTheme.Id;
                }
            }
        }

        private void CreateTheme()
        {
            var defaultColors = _settingsService.GetThemeColors(_defaultValueProvider.GetDefaultThemeId());
            var theme = new TerminalTheme
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New Theme",
                Colors = new TerminalColors(defaultColors)
            };

            _settingsService.SaveTheme(theme);

            var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService);
            viewModel.Activated += OnThemeActivated;
            viewModel.Deleted += OnThemeDeleted;
            Themes.Add(viewModel);
            SelectedTheme = viewModel;
        }

        private void OnThemeDeleted(object sender, EventArgs e)
        {
            if (sender is ThemeViewModel theme)
            {
                if (SelectedTheme == theme)
                {
                    SelectedTheme = Themes.First();
                }
                Themes.Remove(theme);

                if (theme.IsActive)
                {
                    Themes.First().IsActive = true;
                    _settingsService.SaveCurrentThemeId(Themes.First().Id);
                }
                _settingsService.DeleteTheme(theme.Id);   

            }
        }

        public RelayCommand BrowseForCustomShellCommand { get; }
        public RelayCommand BrowseForWorkingDirectoryCommand { get; }
        public RelayCommand<string> RestoreDefaultsCommand { get; }
        public RelayCommand CreateThemeCommand { get; set; }

        public ObservableCollection<ThemeViewModel> Themes { get; } = new ObservableCollection<ThemeViewModel>();

        public IEnumerable<string> Fonts { get; }

        public IEnumerable<int> Sizes { get; }

        public ThemeViewModel SelectedTheme
        {
            get => _selectedTheme;
            set => Set(ref _selectedTheme, value);
        }

        private CursorStyle CursorStyle
        {
            get => _terminalOptions.CursorStyle;
            set
            {
                if (_terminalOptions.CursorStyle != value)
                {
                    _terminalOptions.CursorStyle = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(BlockIsSelected));
                    RaisePropertyChanged(nameof(BarIsSelected));
                    RaisePropertyChanged(nameof(UnderlineIsSelected));
                }
            }
        }

        public bool CursorBlink
        {
            get => _terminalOptions.CursorBlink;
            set
            {
                if (_terminalOptions.CursorBlink != value)
                {
                    _terminalOptions.CursorBlink = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public int FontSize
        {
            get => _terminalOptions.FontSize;
            set
            {
                if (_terminalOptions.FontSize != value)
                {
                    _terminalOptions.FontSize = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public string FontFamily
        {
            get => _terminalOptions.FontFamily;
            set
            {
                if (_terminalOptions.FontFamily != value)
                {
                    _terminalOptions.FontFamily = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public bool BlockIsSelected
        {
            get => CursorStyle == CursorStyle.Block;
            set => CursorStyle = CursorStyle.Block;
        }

        public bool BarIsSelected
        {
            get => CursorStyle == CursorStyle.Bar;
            set => CursorStyle = CursorStyle.Bar;
        }

        public bool UnderlineIsSelected
        {
            get => CursorStyle == CursorStyle.Underline;
            set => CursorStyle = CursorStyle.Underline;
        }

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

        public bool PoweShellIsSelected
        {
            get => ShellType == ShellType.PowerShell;
            set => ShellType = ShellType.PowerShell;
        }

        public ShellType ShellType
        {
            get => _shellConfiguration.Shell;
            set
            {
                if (_shellConfiguration.Shell != value)
                {
                    _shellConfiguration.Shell = value;
                    _settingsService.SaveShellConfiguration(_shellConfiguration);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CustomShellIsSelected));
                    RaisePropertyChanged(nameof(PoweShellIsSelected));
                    RaisePropertyChanged(nameof(CMDIsSelected));
                }
            }
        }

        private async Task BrowseForCustomShell()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".exe");

            var file = await picker.PickSingleFileAsync().AsTask();
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

        private async Task RestoreDefaults(string area)
        {
            var result = await _dialogService.ShowDialogAsnyc("Please confirm", "Are you sure you want to restore default values for this page?", DialogButton.OK, DialogButton.Cancel);

            if (result == DialogButton.OK && area == "shell")
            {
                var defaults = _defaultValueProvider.GetDefaultShellConfiguration();
                ShellType = defaults.Shell;
                CustomShellLocation = defaults.CustomShellLocation;
                WorkingDirectory = defaults.WorkingDirectory;
                Arguments = defaults.Arguments;
            }
            else if(result == DialogButton.OK && area == "terminal")
            {
                var defaults = _defaultValueProvider.GetDefaultTerminalOptions();
                CursorBlink = defaults.CursorBlink;
                CursorStyle = defaults.CursorStyle;
                FontFamily = defaults.FontFamily;
                FontSize = defaults.FontSize;
            }
        }
    }
}