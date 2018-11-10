using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IApplicationView _applicationView;
        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherTimer _dispatcherTimer;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private ApplicationSettings _applicationSettings;
        private string _background;
        private double _backgroundOpacity;
        private TerminalViewModel _selectedTerminal;
        private TabsPosition _tabsPosition;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService,
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            _settingsService.ShellProfileCollectionChanged += OnShellProfileCollectionChanged;

            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            _applicationView = applicationView;
            _dispatcherTimer = dispatcherTimer;
            _clipboardService = clipboardService;
            _keyboardCommandService = keyboardCommandService;
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewTab), () => AddTerminal(null, false));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ConfigurableNewTab), () => AddTerminal(null, true));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.CloseTab), CloseCurrentTab);

            // Add all of the commands for switching to a tab of a given ID, if there's one open there
            for (int i = 0; i < 9; i++)
            {
                var switchCmd = Command.SwitchToTerm1 + i;
                int tabNumber = i;
                void handler() => SelectTabNumber(tabNumber);
                _keyboardCommandService.RegisterCommandHandler(switchCmd.ToString(), handler);
            }

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NextTab), SelectNextTab);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.PreviousTab), SelectPreviousTab);

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewWindow), NewWindow);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ShowSettings), ShowSettings);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ToggleFullScreen), ToggleFullScreen);

            foreach (ShellProfile profile in _settingsService.GetShellProfiles())
            {
                _keyboardCommandService.RegisterCommandHandler(profile.Id.ToString(), () => AddTerminal(profile.WorkingDirectory, false, profile));
            }

            var currentTheme = _settingsService.GetCurrentTheme();
            var options = _settingsService.GetTerminalOptions();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = options.BackgroundOpacity;
            _applicationSettings = _settingsService.GetApplicationSettings();
            TabsPosition = _applicationSettings.TabsPosition;

            AddTerminalCommand = new RelayCommand(() => AddTerminal(null, false));
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            _applicationView.CloseRequested += OnCloseRequest;
            Terminals.CollectionChanged += OnTerminalsCollectionChanged;
        }

        private void OnShellProfileCollectionChanged(object sender, Tuple<bool, ShellProfile> e)
        {
            if (e.Item1) // New Shell, just add it to the command handler.
            {
                _keyboardCommandService.RegisterCommandHandler(e.Item2.Id.ToString(), () => AddTerminal(e.Item2.WorkingDirectory, false, e.Item2));
            }
            else // Existing shell to remove, deregister it.
            {
                _keyboardCommandService.DeregisterCommandHandler(e.Item2.Id.ToString());
            }
        }

        private void OnTerminalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ShowTabsOnTop));
            RaisePropertyChanged(nameof(ShowTabsOnBottom));
        }

        public event EventHandler Closed;

        public event EventHandler NewWindowRequested;

        public event EventHandler ShowSettingsRequested;

        public event EventHandler ShowAboutRequested;

        public RelayCommand AddTerminalCommand { get; }

        public bool ShowTabsOnTop
        {
            get => TabsPosition == TabsPosition.Top && (Terminals.Count > 1 || _applicationSettings.AlwaysShowTabs);
        }

        public bool ShowTabsOnBottom
        {
            get => TabsPosition == TabsPosition.Bottom && (Terminals.Count > 1 || _applicationSettings.AlwaysShowTabs);
        }

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

        public RelayCommand ShowAboutCommand { get; }

        public RelayCommand ShowSettingsCommand { get; }

        public TabsPosition TabsPosition
        {
            get => _tabsPosition;
            set => Set(ref _tabsPosition, value);
        }

        public ObservableCollection<TerminalViewModel> Terminals { get; } = new ObservableCollection<TerminalViewModel>();

        /// <summary>
        /// Add a new terminal window, either with a specified terminal profile, with the default, or by showing the profile selection dialog.
        /// </summary>
        /// <param name="startupDirectory"></param>
        /// <param name="profile"></param>
        /// <param name="showProfileSelection"></param>
        /// <returns></returns>
        public Task AddTerminal(string startupDirectory, bool showProfileSelection, ShellProfile profile = null)
        {
            return _applicationView.RunOnDispatcherThread(async () =>
            {
                if (showProfileSelection)
                {
                    profile = await _dialogService.ShowProfileSelectionDialogAsync();

                    if (profile == null)
                    {
                        return;
                    }
                }
                else if (profile == null)
                {
                    profile = _settingsService.GetDefaultShellProfile();
                }

                var terminal = new TerminalViewModel(_settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService,
                    _applicationSettings, startupDirectory, profile, _applicationView, _dispatcherTimer, _clipboardService);
                terminal.Closed += OnTerminalCloseRequested;
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

        private void NewWindow()
        {
            NewWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void OnApplicationSettingsChanged(object sender, ApplicationSettings e)
        {
            await _applicationView.RunOnDispatcherThread(() =>
            {
                _applicationSettings = e;
                TabsPosition = e.TabsPosition;
                RaisePropertyChanged(nameof(ShowTabsOnTop));
                RaisePropertyChanged(nameof(ShowTabsOnBottom));
            });
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

        private async void OnTerminalOptionsChanged(object sender, TerminalOptions e)
        {
            await _applicationView.RunOnDispatcherThread(() =>
            {
                BackgroundOpacity = e.BackgroundOpacity;
            });
        }

        private void SelectTabNumber(int tabNumber)
        {
            if (tabNumber < Terminals.Count)
            {
                SelectedTerminal = Terminals[tabNumber];
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

        private void ShowAbout()
        {
            ShowAboutRequested?.Invoke(this, EventArgs.Empty);
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