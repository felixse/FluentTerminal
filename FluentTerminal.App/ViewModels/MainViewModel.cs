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
        private const string FallbackTitle = "Fluent Terminal";
        private string _title;

        public MainViewModel(ISettingsService settingsService, ITerminalService terminalService)
        {
            _settingsService = settingsService;
            _terminalService = terminalService;
            Title = FallbackTitle;

            AddTerminalCommand = new RelayCommand(() => AddTerminal(null));
            ShowSettingsCommand = new RelayCommand(async () => await ShowSettings());
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

        private Task ShowSettings()
        {
            return App.Instance.ShowSettings();
        }
    }
}