using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
        private int _nextTerminalId;
        private TerminalViewModel _selectedTerminal;
        private string _title;
        private readonly IApplicationView _applicationView;
        private readonly IDispatcherTimer _dispatcherTimer;
        private readonly IClipboardService _clipboardService;

        public event EventHandler Closed;
        public event EventHandler NewWindowRequested;
        public event EventHandler ShowSettingsRequested;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService,
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            _applicationView = applicationView;
            _dispatcherTimer = dispatcherTimer;
            _clipboardService = clipboardService;
            _keyboardCommandService = keyboardCommandService;
            _keyboardCommandService.RegisterCommandHandler(Command.NewTab, () => AddTerminal(null, _settingsService.GetDefaultShellProfile()));
            _keyboardCommandService.RegisterCommandHandler(Command.ConfigurableNewTab, () => AddTerminal(null, null));
            _keyboardCommandService.RegisterCommandHandler(Command.CloseTab, CloseCurrentTab);
            _keyboardCommandService.RegisterCommandHandler(Command.NextTab, SelectNextTab);
            _keyboardCommandService.RegisterCommandHandler(Command.PreviousTab, SelectPreviousTab);
            _keyboardCommandService.RegisterCommandHandler(Command.NewWindow, NewWindow);
            _keyboardCommandService.RegisterCommandHandler(Command.ShowSettings, ShowSettings);
            _keyboardCommandService.RegisterCommandHandler(Command.ToggleFullScreen, ToggleFullScreen);

            foreach (ShellProfile shellProfile in _settingsService.GetShellProfiles())
            {
                if (shellProfile.KeyBinding != null)
                {
                    _keyboardCommandService.RegisterCommandHandler(shellProfile.KeyBindingCommand, () => AddTerminal(null, shellProfile));
                }
            }

            var currentTheme = _settingsService.GetCurrentTheme();
            var options = _settingsService.GetTerminalOptions();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = options.BackgroundOpacity;
            _applicationSettings = _settingsService.GetApplicationSettings();

            AddTerminalCommand = new RelayCommand(() => AddTerminal(null, _settingsService.GetDefaultShellProfile()));
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            Terminals.CollectionChanged += OnTerminalsCollectionChanged;

            Title = "Fluent Terminal";

            _applicationView.CloseRequested += OnCloseRequest;
        }

        private async void OnTerminalOptionsChanged(object sender, TerminalOptions e)
        {
            await _applicationView.RunOnDispatcherThread(() =>
            {
                BackgroundOpacity = e.BackgroundOpacity;
            });
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
                if (SelectedTerminal != null)
                {
                    _selectedTerminal.IsSelected = false;
                }
                if (Set(ref _selectedTerminal, value))
                {
                    SelectedTerminal?.FocusTerminal();
                    if (SelectedTerminal != null)
                    {
                        SelectedTerminal.IsSelected = true;
                    }
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

        public Task AddTerminal(string startupDirectory, ShellProfile startupProfile)
        {
            return _applicationView.RunOnDispatcherThread(async () =>
            {
                ShellProfile profile = startupProfile;
                if (profile == null)
                {
                    profile = await _dialogService.ShowProfileSelectionDialogAsync();

                    if (profile == null)
                    {
                        return;
                    }
                }

                var terminal = new TerminalViewModel(GetNextTerminalId(), _settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService,
                    _applicationSettings, startupDirectory, profile, _applicationView, _dispatcherTimer, _clipboardService);
                terminal.CloseRequested += OnTerminalCloseRequested;
                terminal.TitleChanged += OnTitleChanged;
                Terminals.Add(terminal);

                SelectedTerminal = terminal;
            });
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

        private async void OnApplicationSettingsChanged(object sender, ApplicationSettings e)
        {
            await _applicationView.RunOnDispatcherThread(() => _applicationSettings = e);
        }

        private async Task OnCloseRequest(object sender, CancelableEventArgs e)
        {
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
                    e.Cancelled = true;
                }
            }
            else
            {
                CloseAllTerminals();
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void OnCurrentThemeChanged(object sender, Guid e)
        {
            await _applicationView.RunOnDispatcherThread(() =>
             {
                 var currentTheme = _settingsService.GetTheme(e);
                 Background = currentTheme.Colors.Background;
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
                    _applicationView.TryClose();
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
      
        private void ToggleFullScreen()
        {
            _applicationView.ToggleFullScreen();
        }
    }
}