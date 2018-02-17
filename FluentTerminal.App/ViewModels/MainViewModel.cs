using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly ITerminalService _terminalService;
        private string _background;
        private double _backgroundOpacity;
        private CoreDispatcher _dispatcher;
        private int _nextTerminalId;
        private TerminalViewModel _selectedTerminal;

        public MainViewModel(ISettingsService settingsService, ITerminalService terminalService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _terminalService = terminalService;
            _dialogService = dialogService;

            var currentTheme = _settingsService.GetCurrentTheme();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = currentTheme.BackgroundOpacity;

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

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => Set(ref _backgroundOpacity, value);
        }

        public TerminalViewModel SelectedTerminal
        {
            get => _selectedTerminal;
            set
            {
                if (Set(ref _selectedTerminal, value))
                {
                    SelectedTerminal?.FocusTerminal();
                }
            }
        }

        public RelayCommand ShowSettingsCommand { get; }

        public ObservableCollection<TerminalViewModel> Terminals { get; } = new ObservableCollection<TerminalViewModel>();

        public void AddTerminal(string startupDirectory)
        {
            var terminal = new TerminalViewModel(GetNextTerminalId(), _settingsService, _terminalService, _dialogService, startupDirectory);
            terminal.CloseRequested += OnTerminalCloseRequested;
            Terminals.Add(terminal);

            SelectedTerminal = terminal;
        }

        public void CloseAllTerminals()
        {
            foreach (var terminal in Terminals)
            {
                terminal.CloseView();
            }
        }

        private int GetNextTerminalId()
        {
            return _nextTerminalId++;
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

        private void OnTerminalCloseRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                terminal.CloseView();
                if (SelectedTerminal == terminal)
                {
                    SelectedTerminal = Terminals.FirstOrDefault(t => t != terminal);
                }
                Terminals.Remove(terminal);
            }
        }

        private Task ShowSettings()
        {
            return App.Instance.ShowSettings();
        }
    }
}