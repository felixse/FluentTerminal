using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const string FallbackTitle = "Fluent Terminal";
        private readonly ISettingsService _settingsService;
        private readonly ITerminalService _terminalService;
        private readonly IDialogService _dialogService;
        private string _background;
        private double _backgroundOpacity;
        private CoreDispatcher _dispatcher;
        private TerminalViewModel _selectedTerminal;
        private string _title;

        public MainViewModel(ISettingsService settingsService, ITerminalService terminalService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _terminalService = terminalService;
            _dialogService = dialogService;
            Title = FallbackTitle;

            var currentTheme = _settingsService.GetCurrentTheme();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = currentTheme.BackgroundOpacity;

            AddTerminalCommand = new RelayCommand(() => AddTerminal(null));
            ShowSettingsCommand = new RelayCommand(async () => await ShowSettings());

            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
        }

        public RelayCommand AddTerminalCommand { get; }

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => Set(ref _backgroundOpacity, value);
        }

        public string Background
        {
            get => _background;
            set => Set(ref _background, value);
        }

        public TerminalViewModel SelectedTerminal
        {
            get => _selectedTerminal;
            set => Set(ref _selectedTerminal, value);
        }

        public RelayCommand ShowSettingsCommand { get; }

        public ObservableCollection<TerminalViewModel> Terminals { get; } = new ObservableCollection<TerminalViewModel>();

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public void AddTerminal(string startupDirectory)
        {
            var terminal = new TerminalViewModel(_settingsService, _terminalService, _dialogService, startupDirectory);
            terminal.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TerminalViewModel.Title) && Terminals.Count == 1)
                {
                    if (string.IsNullOrWhiteSpace(terminal.Title))
                    {
                        Title = FallbackTitle;
                    }
                    else
                    {
                        Title = terminal.Title;
                    }
                }
            };
            Terminals.Add(terminal);

            SelectedTerminal = terminal;
        }

        private async void OnCurrentThemeChanged(object sender, System.EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 var currentTheme = _settingsService.GetCurrentTheme();
                 Background = currentTheme.Colors.Background;
                 BackgroundOpacity = currentTheme.BackgroundOpacity;
             });
        }

        private Task ShowSettings()
        {
            return App.Instance.ShowSettings();
        }
    }
}