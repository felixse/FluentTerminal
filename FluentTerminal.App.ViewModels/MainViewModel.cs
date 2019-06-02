using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.App.Services.Utilities;
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
        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherTimer _dispatcherTimer;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly ISshHelperService _sshHelperService;
        private ApplicationSettings _applicationSettings;
        private string _background;
        private double _backgroundOpacity;
        private TerminalViewModel _selectedTerminal;
        private TabsPosition _tabsPosition;
        private string _windowTitle;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService,
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService, ISshHelperService sshHelperService)
        {
            _settingsService = settingsService;
            _settingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            _settingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            _settingsService.ShellProfileAdded += OnShellProfileAdded;
            _settingsService.ShellProfileDeleted += OnShellProfileDeleted;
            _settingsService.SshShellProfileAdded += OnSshShellProfileAdded;
            _settingsService.SshShellProfileDeleted += OnSshShellProfileDeleted;

            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            ApplicationView = applicationView;
            _dispatcherTimer = dispatcherTimer;
            _clipboardService = clipboardService;
            _sshHelperService = sshHelperService;
            _keyboardCommandService = keyboardCommandService;
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewTab), () => AddTerminal());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewSshTab), () => AddRemoteTerminal());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ConfigurableNewTab), () => AddConfigurableTerminal());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ChangeTabTitle), () => SelectedTerminal.EditTitle());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.CloseTab), CloseCurrentTab);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.SavedSshNewTab), () => AddSavedShhRemoteTerminal());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.SavedSshNewWindow), () => NewWindow(ProfileSelection.ShowSshProfileSelection));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewSshWindow), () => NewWindow(ProfileSelection.ShowNewSshTab));
            
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

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewWindow), () => NewWindow(ProfileSelection.DoNotShowSelection));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ConfigurableNewWindow), () => NewWindow(ProfileSelection.ShowProfileSelection));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ShowSettings), ShowSettings);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ToggleFullScreen), ToggleFullScreen);

            foreach (ShellProfile profile in _settingsService.GetShellProfiles())
            {
                _keyboardCommandService.RegisterCommandHandler(profile.Id.ToString(), () => AddTerminal(profile.Id));
            }

            foreach (SshShellProfile profile in _settingsService.GetSshShellProfiles())
            {
                _keyboardCommandService.RegisterCommandHandler(profile.Id.ToString(), () => AddRemoteTerminal(profile.Id));
            }

            var currentTheme = _settingsService.GetCurrentTheme();
            var options = _settingsService.GetTerminalOptions();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = options.BackgroundOpacity;
            _applicationSettings = _settingsService.GetApplicationSettings();
            TabsPosition = _applicationSettings.TabsPosition;

            AddLocalShellCommand = new RelayCommand(() => AddTerminal());
            AddRemoteShellCommand = new RelayCommand(() => AddRemoteTerminal());
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            ApplicationView.CloseRequested += OnCloseRequest;
            ApplicationView.Closed += OnClosed;
            Terminals.CollectionChanged += OnTerminalsCollectionChanged;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, e);
        }

        private void OnShellProfileDeleted(object sender, Guid e)
        {
            _keyboardCommandService.DeregisterCommandHandler(e.ToString());
        }

        private void OnShellProfileAdded(object sender, ShellProfile e)
        {
            _keyboardCommandService.RegisterCommandHandler(e.Id.ToString(), () => AddTerminal(e.Id));
        }

        private void OnSshShellProfileAdded(object sender, ShellProfile e)
        {
            _keyboardCommandService.RegisterCommandHandler(e.Id.ToString(), () => AddRemoteTerminal(e.Id));
        }
        private void OnSshShellProfileDeleted(object sender, Guid e)
        {
            _keyboardCommandService.DeregisterCommandHandler(e.ToString());
        }
        
        private void OnTerminalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ShowTabsOnTop));
            RaisePropertyChanged(nameof(ShowTabsOnBottom));
        }

        public event EventHandler Closed;

        public event EventHandler<NewWindowRequestedEventArgs> NewWindowRequested;

        public event EventHandler ShowSettingsRequested;

        public event EventHandler ShowAboutRequested;

        public event EventHandler ActivatedMv;

        public void FocusWindow()
        {
            ActivatedMv?.Invoke(this, EventArgs.Empty);
        }

        public RelayCommand AddLocalShellCommand { get; }
        public RelayCommand AddRemoteShellCommand { get; }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (Set(ref _windowTitle, value))
                {
                    ApplicationView.Title = value;
                }
            }
        }

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
                        if (_applicationSettings.ShowCustomTitleInTitlebar)
                        {
                            WindowTitle = SelectedTerminal.TabTitle;
                        }
                        else
                        {
                            WindowTitle = SelectedTerminal.ShellTitle;
                        }
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

        public IApplicationView ApplicationView { get; }

        public Task AddConfigurableTerminal()
        {
            return ApplicationView.RunOnDispatcherThread(async () =>
            {
                var profile = await _dialogService.ShowProfileSelectionDialogAsync();

                if (profile == null)
                {
                    if (Terminals.Count == 0)
                    {
                        await ApplicationView.TryClose();
                    }

                    return;
                }

                AddTerminal(profile);
            });
        }

        public async Task AddRemoteTerminal()
        {
            SshShellProfile profile = await _sshHelperService.GetSshShellProfileAsync(new SshShellProfile());

            if (profile == null)
            {
                // User selected "Cancel"
                if (Terminals.Count == 0)
                {
                    await ApplicationView.TryClose();
                }
            }
            else
            {
                await ApplicationView.RunOnDispatcherThread(() => AddTerminal(profile));
            }
        }
        public void AddRemoteTerminal(Guid shellProfileId)
        {
            var profile = _settingsService.GetSshShellProfile(shellProfileId);
            AddTerminal(profile);
        }

        public async Task AddSavedShhRemoteTerminal()
        {
            ShellProfile profile = await _sshHelperService.GetSavedSshShellProfileAsync();

            if (profile == null)
            {
                // User selected "Cancel"
                if (Terminals.Count == 0)
                {
                    await ApplicationView.TryClose();
                }
            }
            else
            {
                await ApplicationView.RunOnDispatcherThread(() => AddTerminal(profile));
            }
        }

        public void AddTerminal()
        {
            var profile = _settingsService.GetDefaultShellProfile();
            AddTerminal(profile);
        }

        public void AddTerminal(Guid shellProfileId)
        {
            var profile = _settingsService.GetShellProfile(shellProfileId);
            AddTerminal(profile);
        }

        public void AddTerminal(ShellProfile profile)
        {
            ApplicationView.RunOnDispatcherThread(() =>
            {
                var terminal = new TerminalViewModel(_settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService,
                    _applicationSettings, profile, ApplicationView, _dispatcherTimer, _clipboardService);

                terminal.Closed += OnTerminalClosed;
                terminal.ShellTitleChanged += Terminal_ShellTitleChanged;
                terminal.CustomTitleChanged += Terminal_CustomTitleChanged;
                Terminals.Add(terminal);

                SelectedTerminal = terminal;
            });
        }

        private void Terminal_CustomTitleChanged(object sender, string e)
        {
            if (sender is TerminalViewModel terminal)
            {
                if (terminal.IsSelected && _applicationSettings.ShowCustomTitleInTitlebar)
                {
                    if (string.IsNullOrWhiteSpace(e))
                    {
                        WindowTitle = terminal.ShellTitle;
                    }
                    else
                    {
                        WindowTitle = e;
                    }
                }
            }
        }

        private void Terminal_ShellTitleChanged(object sender, string e)
        {
            if (sender is TerminalViewModel terminal && terminal.IsSelected && !_applicationSettings.ShowCustomTitleInTitlebar)
            {
                WindowTitle = e;
            }
        }

        public async Task CloseAllTerminals()
        {
            foreach (var terminal in Terminals)
            {
                terminal.Closed -= OnTerminalClosed;
                terminal.ShellTitleChanged -= Terminal_ShellTitleChanged;
                terminal.CustomTitleChanged -= Terminal_CustomTitleChanged;
                await terminal.Close();
            }
        }

        private void CloseCurrentTab()
        {
            SelectedTerminal?.CloseCommand.Execute(null);
        }

        private void NewWindow(ProfileSelection showSelection)
        {
            var args = new NewWindowRequestedEventArgs
            {
                ShowSelection = showSelection
            };

            NewWindowRequested?.Invoke(this, args);
        }

        private async void OnApplicationSettingsChanged(object sender, ApplicationSettings e)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
            {
                _applicationSettings = e;
                TabsPosition = e.TabsPosition;
                RaisePropertyChanged(nameof(ShowTabsOnTop));
                RaisePropertyChanged(nameof(ShowTabsOnBottom));

                if (e.ShowCustomTitleInTitlebar)
                {
                    WindowTitle = SelectedTerminal?.TabTitle;
                }
                else
                {
                    WindowTitle = SelectedTerminal?.ShellTitle;
                }
            });
        }

        private async Task OnCloseRequest(object sender, CancelableEventArgs e)
        {
            if (_applicationSettings.ConfirmClosingWindows)
            {
                var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmCloseWindow"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(false);

                if (result == DialogButton.OK)
                {
                    await CloseAllTerminals();
                }
                else
                {
                    e.Cancelled = true;
                }
            }
            else
            {
                await CloseAllTerminals();
            }
        }

        private async void OnCurrentThemeChanged(object sender, Guid e)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
             {
                 var currentTheme = _settingsService.GetTheme(e);
                 Background = currentTheme.Colors.Background;
             });
        }

        private async void OnTerminalClosed(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                if (SelectedTerminal == terminal)
                {
                    SelectedTerminal = Terminals.LastOrDefault(t => t != terminal);
                }
                Logger.Instance.Debug("Terminal with Id: {@id} closed.", terminal.Terminal.Id);
                Terminals.Remove(terminal);

                if (Terminals.Count == 0)
                {
                    await ApplicationView.TryClose();
                }
            }
        }

        private async void OnTerminalOptionsChanged(object sender, TerminalOptions e)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
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
            ApplicationView.ToggleFullScreen();
        }
    }
}