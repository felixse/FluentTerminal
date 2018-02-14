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
        private string _background;
        private CoreDispatcher _dispatcher;
        private TerminalViewModel _selectedTerminal;
        private string _title;

        public MainViewModel(ISettingsService settingsService, ITerminalService terminalService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _terminalService = terminalService;
            Title = FallbackTitle;
            Background = _settingsService.GetCurrentThemeColors().Background;

            AddTerminalCommand = new RelayCommand(() => AddTerminal(null));
            ShowSettingsCommand = new RelayCommand(async () => await ShowSettings());

            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
        }

        public RelayCommand AddTerminalCommand { get; }

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
            var terminal = new TerminalViewModel(_settingsService, _terminalService, startupDirectory);
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
                 Background = _settingsService.GetCurrentThemeColors().Background;
             });
        }

        private Task ShowSettings()
        {
            return App.Instance.ShowSettings();
        }
    }
}