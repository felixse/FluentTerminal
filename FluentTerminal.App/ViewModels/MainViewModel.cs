using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ITerminalService _terminalService;
        private TerminalViewModel _selectedTerminal;
        private string _title;

        public MainViewModel(ISettingsService settingsService, ITerminalService terminalService)
        {
            _settingsService = settingsService;
            _terminalService = terminalService;
            Title = "Fluent Terminal";

            AddTerminalCommand = new RelayCommand(AddTerminal);
            ShowSettingsCommand = new RelayCommand(async () => await ShowSettings());

            AddTerminal();
        }

        public RelayCommand AddTerminalCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }

        public TerminalViewModel SelectedTerminal
        {
            get => _selectedTerminal;
            set => Set(ref _selectedTerminal, value);
        }

        public ObservableCollection<TerminalViewModel> Terminals { get; } = new ObservableCollection<TerminalViewModel>();

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private void AddTerminal()
        {
            var terminal = new TerminalViewModel(_settingsService, _terminalService);
            terminal.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TerminalViewModel.Title) && Terminals.Count == 1)
                {
                    Title = terminal.Title;
                }
            };
            Terminals.Add(terminal);

            SelectedTerminal = terminal;
        }

        private Task ShowSettings()
        {
            return App.Instance.ShowSettings();
        }
    }
}