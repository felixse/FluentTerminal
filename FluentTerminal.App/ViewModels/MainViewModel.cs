using FluentTerminal.App.Dialogs;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private ApplicationSettings _applicationSettings;
        private string _background;
        private double _backgroundOpacity;
        private readonly CoreDispatcher _dispatcher;
        private int _nextTerminalId;
        private TerminalViewModel _selectedTerminal;
        private string _title;

        public event EventHandler Closed;
        public event EventHandler NewWindowRequested;
        public event EventHandler ShowSettingsRequested;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            _keyboardCommandService.RegisterCommandHandler(Command.NewTab, () => AddTerminal(null, false));
            _keyboardCommandService.RegisterCommandHandler(Command.ConfigurableNewTab, () => AddTerminal(null, true));
            _keyboardCommandService.RegisterCommandHandler(Command.CloseTab, CloseCurrentTab);
            _keyboardCommandService.RegisterCommandHandler(Command.NextTab, SelectNextTab);
            _keyboardCommandService.RegisterCommandHandler(Command.PreviousTab, SelectPreviousTab);
            _keyboardCommandService.RegisterCommandHandler(Command.NewWindow, NewWindow);
            _keyboardCommandService.RegisterCommandHandler(Command.ShowSettings, ShowSettings);
            var currentTheme = _settingsService.GetCurrentTheme();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = currentTheme.BackgroundOpacity;
            _applicationSettings = _settingsService.GetApplicationSettings();

            AddTerminalCommand = new RelayCommand(() => AddTerminal(null, false));
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            Terminals.CollectionChanged += OnTerminalsCollectionChanged;

            Title = "Fluent Terminal";

            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
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

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public Task AddTerminal(string startupDirectory, bool showProfileSelection)
        {
            return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ShellProfile profile = null;
                if (showProfileSelection)
                {
                    profile = await _dialogService.ShowProfileSelectionDialogAsync();

                    if (profile == null)
                    {
                        return;
                    }
                }
                else
                {
                    profile = _settingsService.GetDefaultShellProfile();
                }

                var terminal = new TerminalViewModel(GetNextTerminalId(), _settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService, _applicationSettings, startupDirectory, profile);
                terminal.CloseRequested += OnTerminalCloseRequested;
                terminal.TitleChanged += OnTitleChanged;
                Terminals.Add(terminal);

                SelectedTerminal = terminal;
            }).AsTask();
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

        private void NewWindow()
        {
            NewWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void OnApplicationSettingsChanged(object sender, EventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _applicationSettings = _settingsService.GetApplicationSettings());
        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();

            if (_applicationSettings.ConfirmClosingWindows)
            {
                var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to close this window?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(false);

                if (result == DialogButton.OK)
                {
                    CloseAllTerminals();
                    Closed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                CloseAllTerminals();
                Closed?.Invoke(this, EventArgs.Empty);
            }
            deferral.Complete();
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
                    SelectedTerminal = Terminals.LastOrDefault(t => t != terminal);
                }
                Terminals.Remove(terminal);

                if (Terminals.Count == 0)
                {
                    Closed?.Invoke(this, EventArgs.Empty);
                    ApplicationView.GetForCurrentView().TryConsolidateAsync();
                }
            }
        }

        private void OnTerminalsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Terminals.Any())
            {
                Title = Terminals.First().Title;
            }
        }

        private void OnTitleChanged(object sender, string e)
        {
            if (sender is TerminalViewModel terminal && Terminals.First() == terminal)
            {
                Title = e;
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

        private void ShowSettings()
        {
            ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}