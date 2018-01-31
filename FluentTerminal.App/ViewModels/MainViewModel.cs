using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private TerminalViewModel _selectedTerminal;
        private string _title;

        public MainViewModel()
        {
            Title = "Fluent Terminal";

            AddTerminalCommand = new RelayCommand(AddTerminal);

            AddTerminal();
        }

        public RelayCommand AddTerminalCommand { get; }

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
            var terminal = new TerminalViewModel();
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
    }
}