﻿using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.Models.Messages;

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
        private readonly IImageFileSystemService _imageFileSystemService;

        public event EventHandler<string> SelectedThemeBackgroundColorChanged;
        public event EventHandler<ImageFile> SelectedThemeBackgroundImageChanged;
        public event EventHandler<ThemeViewModel> SelectedThemeChanged;

        public ThemesPageViewModel(ISettingsService settingsService,
                                   IDialogService dialogService,
                                   IDefaultValueProvider defaultValueProvider,
                                   IThemeParserFactory themeParserFactory,
                                   IFileSystemService fileSystemService,
                                   IImageFileSystemService imageFileSystemService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _themeParserFactory = themeParserFactory;
            _fileSystemService = fileSystemService;
            _imageFileSystemService = imageFileSystemService;

            CreateThemeCommand = new RelayCommand(CreateTheme);
            ImportThemeCommand = new AsyncCommand(ImportThemeAsync);
            CloneCommand = new RelayCommand<ThemeViewModel>(CloneTheme);

            MessengerInstance.Register<TerminalOptionsChangedMessage>(this, OnTerminalOptionsChanged);

            BackgroundOpacity = _settingsService.GetTerminalOptions().BackgroundOpacity;

            var activeThemeId = _settingsService.GetCurrentThemeId();
            foreach (var theme in _settingsService.GetThemes())
            {
                var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService, _fileSystemService, _imageFileSystemService, false);
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
        public IAsyncCommand ImportThemeCommand { get; }
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
                    _selectedTheme.BackgroundImageChanged -= OnSelectedThemeBackgroundImageChanged;
                }

                Set(ref _selectedTheme, value);
                SelectedThemeChanged?.Invoke(this, _selectedTheme);

                if (value != null)
                {
                    _selectedTheme.BackgroundOpacity = BackgroundOpacity;
                    value.BackgroundChanged += OnSelectedThemeBackgroundChanged;
                    value.BackgroundImageChanged += OnSelectedThemeBackgroundImageChanged;
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

        // Requires UI thread
        private async Task ImportThemeAsync()
        {
            // ConfigureAwait(true) because we need to execute AddTheme method in the calling (UI) thread.
            var file = await _fileSystemService.OpenFileAsync(_themeParserFactory.SupportedFileTypes)
                .ConfigureAwait(true);

            if (file != null)
            {
                var parser = _themeParserFactory.GetParser(file.FileType);

                if (parser == null)
                {
                    await _dialogService.ShowMessageDialogAsync(I18N.Translate("ImportThemeFailed"),
                        I18N.Translate("NoSuitableParserFound"), DialogButton.OK).ConfigureAwait(false);

                    return;
                }

                try
                {
                    // ConfigureAwait(true) because we need to execute AddTheme method in the calling (UI) thread.
                    var exportedTheme = await parser.Import(file.Name, file.Content).ConfigureAwait(true);

                    if (!string.IsNullOrWhiteSpace(exportedTheme.EncodedImage))
                    {
                        // ConfigureAwait(true) because we need to execute AddTheme method in the calling (UI) thread.
                        var importedImage = await _imageFileSystemService
                            .ImportThemeImageAsync(exportedTheme.BackgroundImage, exportedTheme.EncodedImage)
                            .ConfigureAwait(true);

                        exportedTheme.BackgroundImage = importedImage;
                    }

                    var terminalTheme = new TerminalTheme(exportedTheme);

                    AddTheme(terminalTheme);
                }
                catch (Exception exception)
                {
                    await _dialogService
                        .ShowMessageDialogAsync(I18N.Translate("ImportThemeFailed"), exception.Message, DialogButton.OK)
                        .ConfigureAwait(false);
                }
            }
        }

        private void AddTheme(TerminalTheme theme)
        {
            _settingsService.SaveTheme(theme, true);

            var viewModel = new ThemeViewModel(theme, _settingsService, _dialogService, _fileSystemService, _imageFileSystemService, true);
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

        private void OnTerminalOptionsChanged(TerminalOptionsChangedMessage message)
        {
            BackgroundOpacity = message.TerminalOptions.BackgroundOpacity;
        }

        private void OnSelectedThemeBackgroundChanged(object sender, string e)
        {
            SelectedThemeBackgroundColorChanged?.Invoke(this, e);
        }

        private void OnSelectedThemeBackgroundImageChanged(object sender, ImageFile e)
        {
            SelectedThemeBackgroundImageChanged?.Invoke(this, e);
        }
    }
}