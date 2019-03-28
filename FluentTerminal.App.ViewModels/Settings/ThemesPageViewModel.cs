﻿using FluentTerminal.App.Services;
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
        private double _backgroundOpacity;
        private readonly IThemeParserFactory _themeParserFactory;
        private readonly IFileSystemService _fileSystemService;

        public event EventHandler<string> SelectedThemeBackgroundColorChanged;

        public ThemesPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider,
            IThemeParserFactory themeParserFactory, IFileSystemService fileSystemService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _themeParserFactory = themeParserFactory;
            _fileSystemService = fileSystemService;

            CreateThemeCommand = new RelayCommand(CreateTheme);
            ImportThemeCommand = new RelayCommand(ImportTheme);
            CloneCommand = new RelayCommand<ThemeViewModel>(CloneTheme);

            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;

            BackgroundOpacity = _settingsService.GetTerminalOptions().BackgroundOpacity;

            var activeThemeId = _settingsService.GetCurrentThemeId();
            foreach (var theme in _settingsService.GetThemes())
            {
                var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService, fileSystemService, false);
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

        public RelayCommand CreateThemeCommand { get; }
        public RelayCommand ImportThemeCommand { get; }
        public RelayCommand<ThemeViewModel> CloneCommand { get; set; }

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => Set(ref _backgroundOpacity, value);
        }

        public ThemeViewModel SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != null)
                {
                    _selectedTheme.BackgroundChanged -= OnSelectedThemeBackgroundChanged;
                }
                Set(ref _selectedTheme, value);
                if (value != null)
                {
                    value.BackgroundChanged += OnSelectedThemeBackgroundChanged;
                }
            }
        }

        public ObservableCollection<ThemeViewModel> Themes { get; } = new ObservableCollection<ThemeViewModel>();

        private void CloneTheme(ThemeViewModel theme)
        {
            var cloned = new TerminalTheme(theme.Model)
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = $"Copy of {theme.Name}"
            };

            AddTheme(cloned);
        }

        private void CreateTheme()
        {
            var defaultTheme = _settingsService.GetTheme(_defaultValueProvider.GetDefaultThemeId());
            var theme = new TerminalTheme
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New Theme",
                Colors = new TerminalColors(defaultTheme.Colors)
            };

            AddTheme(theme);
        }

        private async void ImportTheme()
        {
            var file = await _fileSystemService.OpenFile(_themeParserFactory.SupportedFileTypes).ConfigureAwait(true);
            if (file != null)
            {
                var parser = _themeParserFactory.GetParser(file.FileType);

                if (parser == null)
                {
                    await _dialogService.ShowMessageDialogAsnyc("Import theme failed", "No suitable parser found", DialogButton.OK).ConfigureAwait(false);
                    return;
                }

                try
                {
                    var theme = await parser.Parse(file.Name, file.Content).ConfigureAwait(true);

                    AddTheme(theme);
                }
                catch (Exception exception)
                {
                    await _dialogService.ShowMessageDialogAsnyc("Import theme failed", exception.Message, DialogButton.OK).ConfigureAwait(false);
                }
            }
        }

        private void AddTheme(TerminalTheme theme)
        {
            _settingsService.SaveTheme(theme, true);

            var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService, _fileSystemService, true);
            viewModel.EditCommand.Execute(null);
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

        private void OnTerminalOptionsChanged(object sender, TerminalOptions e)
        {
            BackgroundOpacity = e.BackgroundOpacity;
        }

        private void OnSelectedThemeBackgroundChanged(object sender, string e)
        {
            SelectedThemeBackgroundColorChanged?.Invoke(this, e);
        }
    }
}