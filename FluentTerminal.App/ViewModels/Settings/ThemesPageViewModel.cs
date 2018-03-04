using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class ThemesPageViewModel : ViewModelBase
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private ThemeViewModel _selectedTheme;

        public ThemesPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;

            CreateThemeCommand = new RelayCommand(CreateTheme);

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

            SelectedTheme = Themes.First(t => t.IsActive);
        }

        public RelayCommand CreateThemeCommand { get; set; }

        public ThemeViewModel SelectedTheme
        {
            get => _selectedTheme;
            set => Set(ref _selectedTheme, value);
        }

        public ObservableCollection<ThemeViewModel> Themes { get; } = new ObservableCollection<ThemeViewModel>();

        private void CreateTheme()
        {
            var defaultTheme = _settingsService.GetTheme(_defaultValueProvider.GetDefaultThemeId());
            var theme = new TerminalTheme
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New Theme",
                BackgroundOpacity = defaultTheme.BackgroundOpacity,
                Colors = new TerminalColors(defaultTheme.Colors)
            };

            _settingsService.SaveTheme(theme);

            var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService);
            viewModel.Activated += OnThemeActivated;
            viewModel.Deleted += OnThemeDeleted;
            Themes.Add(viewModel);
            SelectedTheme = viewModel;
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
    }
}