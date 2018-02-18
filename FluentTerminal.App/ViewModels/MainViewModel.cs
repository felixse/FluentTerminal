using FluentTerminal.App.Services;
using FluentTerminal.Models.Enums;
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
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITerminalService _terminalService;
        private string _background;
        private double _backgroundOpacity;
        private CoreDispatcher _dispatcher;
        private int _nextTerminalId;
        private TerminalViewModel _selectedTerminal;

        public MainViewModel(ISettingsService settingsService, ITerminalService terminalService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _terminalService = terminalService;
            _dialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            _keyboardCommandService.RegisterCommandHandler(Command.NewTab, () => AddTerminal(null));
            _keyboardCommandService.RegisterCommandHandler(Command.CloseTab, CloseCurrentTab);
            _keyboardCommandService.RegisterCommandHandler(Command.NextTab, SelectNextTab);
            _keyboardCommandService.RegisterCommandHandler(Command.PreviousTab, SelectPreviousTab);
            _keyboardCommandService.RegisterCommandHandler(Command.NewWindow, async () => await NewWindow());
            _keyboardCommandService.RegisterCommandHandler(Command.ShowSettings, async () => await ShowSettings());
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
            var terminal = new TerminalViewModel(GetNextTerminalId(), _settingsService, _terminalService, _dialogService, _keyboardCommandService, startupDirectory);
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

        private void CloseCurrentTab()
        {
            SelectedTerminal?.CloseCommand.Execute(null);
        }

        private int GetNextTerminalId()
        {
            return _nextTerminalId++;
        }

        private Task NewWindow()
        {
            return App.Instance.CreateNewTerminalWindow(null);
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
            if (Terminals.Count == 1)
            {
                return;
            }

            if (sender is TerminalViewModel terminal)
            {
                terminal.CloseView();
                if (SelectedTerminal == terminal)
                {
                    SelectedTerminal = Terminals.LastOrDefault(t => t != terminal);
                }
                Terminals.Remove(terminal);
            }
        }

        private void SelectNextTab()
        {
            var currentIndex = Terminals.IndexOf(SelectedTerminal);
            var nextIndex = (currentIndex + 1) % Terminals.Count;
            SelectedTerminal = Terminals[nextIndex];
        }

        private void SelectPreviousTab()
        {
            var currentIndex = Terminals.IndexOf(SelectedTerminal);
            var previousIndex = (currentIndex - 1 + Terminals.Count) % Terminals.Count;
            SelectedTerminal = Terminals[previousIndex];
        }

        private Task ShowSettings()
        {
            return App.Instance.ShowSettings();
        }
    }
}