using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.Models.Messages;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constants

        private const int RecentItemsMaxCount = 10;

        #endregion Constants

        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherTimer _dispatcherTimer;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly ICommandHistoryService _commandHistoryService;
        private ApplicationSettings _applicationSettings;
        private string _background;
        private double _backgroundOpacity;
        private TerminalViewModel _selectedTerminal;
        private TabsPosition _tabsPosition;
        private string _windowTitle;
        private List<KeyBinding> _keyBindings;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService, 
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService, ICommandHistoryService commandHistoryService)
        {
            MessengerInstance.Register<ApplicationSettingsChangedMessage>(this, OnApplicationSettingsChanged);
            MessengerInstance.Register<CurrentThemeChangedMessage>(this, OnCurrentThemeChanged);
            MessengerInstance.Register<ShellProfileAddedMessage>(this, OnShellProfileAdded);
            MessengerInstance.Register<ShellProfileDeletedMessage>(this, OnShellProfileDeleted);
            MessengerInstance.Register<SshProfileAddedMessage>(this, OnSshProfileAdded);
            MessengerInstance.Register<SshProfileDeletedMessage>(this, OnSshProfileDeleted);
            MessengerInstance.Register<TerminalOptionsChangedMessage>(this, OnTerminalOptionsChanged);
            MessengerInstance.Register<CommandHistoryChangedMessage>(this, OnCommandHistoryChanged);

            _settingsService = settingsService;

            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            ApplicationView = applicationView;
            _dispatcherTimer = dispatcherTimer;
            _clipboardService = clipboardService;
            _keyboardCommandService = keyboardCommandService;
            _commandHistoryService = commandHistoryService;

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewTab), async () => await AddDefaultProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewWindow), async () => await AddDefaultProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewSshTab), async () => await AddSshProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewSshWindow), async () => await AddSshProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewCustomCommandTab), async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewCustomCommandWindow), async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ConfigurableNewTab), async () => await AddSelectedProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ConfigurableNewWindow), async () => await AddSelectedProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ChangeTabTitle), async () => await SelectedTerminal.EditTitle());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.CloseTab), CloseCurrentTab);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.DuplicateTab), async () => await AddTabAsync(SelectedTerminal.ShellProfile.Clone()));

            // Add all of the commands for switching to a tab of a given ID, if there's one open there
            for (int i = 0; i < 9; i++)
            {
                var switchCmd = Command.SwitchToTerm1 + i;
                int tabNumber = i;
                // ReSharper disable once InconsistentNaming
                void handler() => SelectTabNumber(tabNumber);
                _keyboardCommandService.RegisterCommandHandler(switchCmd.ToString(), handler);
            }

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NextTab), SelectNextTab);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.PreviousTab), SelectPreviousTab);

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ShowSettings), ShowSettings);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ToggleFullScreen), ToggleFullScreen);

            foreach (ShellProfile profile in _settingsService.GetShellProfiles())
            {
                _keyboardCommandService.RegisterCommandHandler(profile.Id.ToString(), async () => await AddProfileByGuidAsync(profile.Id));
            }

            foreach (SshProfile profile in _settingsService.GetSshProfiles())
            {
                _keyboardCommandService.RegisterCommandHandler(profile.Id.ToString(), async () => await AddProfileByGuidAsync(profile.Id));
            }

            var currentTheme = _settingsService.GetCurrentTheme();
            var options = _settingsService.GetTerminalOptions();
            Background = currentTheme.Colors.Background;
            BackgroundOpacity = options.BackgroundOpacity;
            _applicationSettings = _settingsService.GetApplicationSettings();
            TabsPosition = _applicationSettings.TabsPosition;

            AddLocalShellCommand = new RelayCommand(async () => await AddDefaultProfileAsync(NewTerminalLocation.Tab));
            AddSshShellCommand = new RelayCommand(async () => await AddSshProfileAsync(NewTerminalLocation.Tab));
            AddQuickShellCommand = new RelayCommand(async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Tab));
            ShowAboutCommand = new AsyncCommand(ShowAbout);
            ShowSettingsCommand = new RelayCommand(ShowSettings);

            ApplicationView.CloseRequested += OnCloseRequest;
            ApplicationView.Closed += OnClosed;
            Terminals.CollectionChanged += OnTerminalsCollectionChanged;

            LoadKeyBindings();
            MessengerInstance.Register<KeyBindingsChangedMessage>(this, message => LoadKeyBindings());
        }

        private IList<ProfileCommandViewModel> _recentCommands;

        public IList<ProfileCommandViewModel> RecentCommands
        {
            get => _recentCommands ?? (_recentCommands = _commandHistoryService
                       .GetHistoryRecentFirst(top: RecentItemsMaxCount).Select(c =>
                           new ProfileCommandViewModel(c.ShellProfile,
                               new RelayCommand(() => AddTabAsync(c.ShellProfile)))).ToList());
            private set => Set(ref _recentCommands, value);
        }

        private void OnCommandHistoryChanged(CommandHistoryChangedMessage message)
        {
            _recentCommands = null;

            ApplicationView.RunOnDispatcherThread(() => RaisePropertyChanged(nameof(RecentCommands)), false);
        }

        private void LoadKeyBindings() =>
            _keyBindings = _settingsService.GetCommandKeyBindings().Values.SelectMany(v => v).ToList();

        private void OnClosed(object sender, EventArgs e)
        {
            MessengerInstance.Unregister(this);

            ApplicationView.CloseRequested -= OnCloseRequest;
            ApplicationView.Closed -= OnClosed;
            Terminals.CollectionChanged -= OnTerminalsCollectionChanged;

            _keyboardCommandService.ClearCommandHandlers();

            _applicationSettings = null;

            AddLocalShellCommand = null;
            AddSshShellCommand = null;
            AddQuickShellCommand = null;
            ShowAboutCommand = null;
            ShowSettingsCommand = null;

            Closed?.Invoke(this, e);
        }

        private void OnShellProfileDeleted(ShellProfileDeletedMessage message)
        {
            _keyboardCommandService.DeregisterCommandHandler(message.ProfileId.ToString());
        }

        private void OnShellProfileAdded(ShellProfileAddedMessage message)
        {
            _keyboardCommandService.RegisterCommandHandler(message.ShellProfile.Id.ToString(),
                async () => await AddProfileByGuidAsync(message.ShellProfile.Id));
        }

        private void OnSshProfileAdded(SshProfileAddedMessage message)
        {
            _keyboardCommandService.RegisterCommandHandler(message.SshProfile.Id.ToString(),
                async () => await AddProfileByGuidAsync(message.SshProfile.Id));
        }
        private void OnSshProfileDeleted(SshProfileDeletedMessage message)
        {
            _keyboardCommandService.DeregisterCommandHandler(message.ProfileId.ToString());
        }
        
        private void OnTerminalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ShowTabsOnTop));
            RaisePropertyChanged(nameof(ShowTabsOnBottom));
        }

        public event EventHandler Closed;

        public event EventHandler<NewWindowRequestedEventArgs> NewWindowRequested;

        public event EventHandler ShowSettingsRequested;

        public event EventHandler ActivatedMv;

        public event EventHandler<TerminalViewModel> TabTearedOff;

        public void TearOffTab(TerminalViewModel model)
        {
            TabTearedOff?.Invoke(this, model);
        }

        public void FocusWindow()
        {
            ActivatedMv?.Invoke(this, EventArgs.Empty);
        }

        public RelayCommand AddLocalShellCommand { get; private set; }
        public RelayCommand AddSshShellCommand { get; private set; }
        public RelayCommand AddQuickShellCommand { get; private set; }

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
            get => TabsPosition == TabsPosition.Top;
        }

        public bool ShowTabsOnBottom
        {
            get => TabsPosition == TabsPosition.Bottom;
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

        public IAsyncCommand ShowAboutCommand { get; private set; }

        public RelayCommand ShowSettingsCommand { get; private set; }

        public TabsPosition TabsPosition
        {
            get => _tabsPosition;
            set => Set(ref _tabsPosition, value);
        }

        public ObservableCollection<TerminalViewModel> Terminals { get; } = new ObservableCollection<TerminalViewModel>();

        public IApplicationView ApplicationView { get; }

        #region Launching terminal sessions

        #region Default profile

        private Task AddDefaultProfileAsync() =>
            AddDefaultProfileAsync(_settingsService.GetApplicationSettings().NewTerminalLocation);

        public Task AddDefaultProfileAsync(NewTerminalLocation location)
        {
            var profile = _settingsService.GetDefaultShellProfile();

            return AddProfileAsync(profile, location);
        }

        #endregion Default profile

        #region User-selected profile

        private Task AddSelectedProfileAsync() =>
            AddSelectedProfileAsync(_settingsService.GetApplicationSettings().NewTerminalLocation);

        private async Task AddSelectedProfileAsync(NewTerminalLocation location)
        {
            var profile = await _dialogService.ShowProfileSelectionDialogAsync();

            await AddProfileAsync(profile, location);
        }

        #endregion User-selected profile

        #region SSH profile

        private Task AddSshProfileAsync() =>
            AddSshProfileAsync(_settingsService.GetApplicationSettings().NewTerminalLocation);

        private async Task AddSshProfileAsync(NewTerminalLocation location)
        {
            var profile = await _dialogService.ShowSshConnectionInfoDialogAsync();

            await AddProfileAsync(profile, location);
        }

        #endregion SSH profile

        #region Quick launch profile

        private Task AddQuickLaunchProfileAsync() =>
            AddQuickLaunchProfileAsync(_settingsService.GetApplicationSettings().NewTerminalLocation);

        private async Task AddQuickLaunchProfileAsync(NewTerminalLocation location)
        {
            var profile = await _dialogService.ShowCustomCommandDialogAsync();

            await AddProfileAsync(profile, location);
        }

        #endregion Quick launch profile

        #region Profile by Guid

        public Task AddProfileByGuidAsync(Guid profileId) =>
            AddProfileByGuidAsync(profileId, _settingsService.GetApplicationSettings().NewTerminalLocation);

        private async Task AddProfileByGuidAsync(Guid profileId, NewTerminalLocation location)
        {
            var profile = _settingsService.GetShellProfile(profileId) ?? _settingsService.GetSshProfile(profileId);

            await AddProfileAsync(profile, location);
        }

        #endregion Profile by Guid

        #region For serialization

        public Task AddTabAsync(string terminalState, int position)
        {
            return AddTabAsync(new ShellProfile(), terminalState, position);
        }

        #endregion For serialization

        private Task AddProfileAsync(ShellProfile profile, NewTerminalLocation location)
        {
            if (profile == null)
            {
                return Task.CompletedTask;
            }

            if (location == NewTerminalLocation.Tab)
            {
                return AddTabAsync(profile);
            }

            NewWindowRequested?.Invoke(this, new NewWindowRequestedEventArgs(profile));

            return Task.CompletedTask;
        }

        public Task AddTabAsync(ShellProfile profile) => AddTabAsync(profile, string.Empty, Terminals.Count);

        private Task AddTabAsync(ShellProfile profile, string terminalState, int position)
        {
            profile.Tag = new DelayedHistorySaver(() => _commandHistoryService.MarkUsed(profile));

            return ApplicationView.RunOnDispatcherThread(() =>
            {
                var terminal = new TerminalViewModel(_settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService,
                    _applicationSettings, profile, ApplicationView, _dispatcherTimer, _clipboardService, terminalState);

                terminal.Closed += OnTerminalClosed;
                terminal.ShellTitleChanged += Terminal_ShellTitleChanged;
                terminal.CustomTitleChanged += Terminal_CustomTitleChanged;
                terminal.CloseLeftTabsRequested += Terminal_CloseLeftTabsRequested;
                terminal.CloseRightTabsRequested += Terminal_CloseRightTabsRequested;
                terminal.CloseOtherTabsRequested += Terminal_CloseOtherTabsRequested;
                terminal.DuplicateTabRequested += Terminal_DuplicateTabRequested;
                Terminals.Insert(Math.Min(position, Terminals.Count), terminal);

                SelectedTerminal = terminal;
            });
        }

        #endregion Launching terminal sessions

        private async void Terminal_DuplicateTabRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                await AddTabAsync(terminal.ShellProfile.Clone());
            }
        }

        private void Terminal_CloseOtherTabsRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                Array.ForEach(Terminals.ToArray(),
                    async t => {
                        if (terminal != t)
                        {
                            Logger.Instance.Debug("Terminal with Id: {@id} closed.", t.Terminal.Id);
                            await t.CloseCommand.ExecuteAsync();
                        }
                    });
            }
        }

        private async void Terminal_CloseRightTabsRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                for (int i = Terminals.Count - 1; i > Terminals.IndexOf(terminal); --i)
                {
                    var terminalToRemove = Terminals[i];
                    Logger.Instance.Debug("Terminal with Id: {@id} closed.", terminalToRemove.Terminal.Id);
                    await terminalToRemove.CloseCommand.ExecuteAsync();
                }
            }
        }

        private async void Terminal_CloseLeftTabsRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                for(int i = Terminals.IndexOf(terminal) - 1; i >= 0; --i)
                {
                    var terminalToRemove = Terminals[i];
                    Logger.Instance.Debug("Terminal with Id: {@id} closed.", terminalToRemove.Terminal.Id);
                    await terminalToRemove.CloseCommand.ExecuteAsync();
                }
            }
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
                terminal.CloseLeftTabsRequested -= Terminal_CloseLeftTabsRequested;
                terminal.CloseRightTabsRequested -= Terminal_CloseRightTabsRequested;
                terminal.CloseOtherTabsRequested -= Terminal_CloseOtherTabsRequested;
                terminal.DuplicateTabRequested -= Terminal_DuplicateTabRequested;
                await terminal.Close();
            }
        }

        private void CloseCurrentTab()
        {
            SelectedTerminal?.CloseCommand.ExecuteAsync();
        }

        private async void OnApplicationSettingsChanged(ApplicationSettingsChangedMessage message)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
            {
                _applicationSettings = message.ApplicationSettings;
                TabsPosition = message.ApplicationSettings.TabsPosition;
                RaisePropertyChanged(nameof(ShowTabsOnTop));
                RaisePropertyChanged(nameof(ShowTabsOnBottom));

                if (message.ApplicationSettings.ShowCustomTitleInTitlebar)
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

        private async void OnCurrentThemeChanged(CurrentThemeChangedMessage message)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
            {
                var currentTheme = _settingsService.GetTheme(message.ThemeId);
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

                terminal.Closed -= OnTerminalClosed;
                terminal.ShellTitleChanged -= Terminal_ShellTitleChanged;
                terminal.CustomTitleChanged -= Terminal_CustomTitleChanged;
                terminal.CloseLeftTabsRequested -= Terminal_CloseLeftTabsRequested;
                terminal.CloseRightTabsRequested -= Terminal_CloseRightTabsRequested;
                terminal.CloseOtherTabsRequested -= Terminal_CloseOtherTabsRequested;
                terminal.DuplicateTabRequested -= Terminal_DuplicateTabRequested;

                Terminals.Remove(terminal);

                if (Terminals.Count == 0)
                {
                    await ApplicationView.TryClose();
                }
            }
        }

        private async void OnTerminalOptionsChanged(TerminalOptionsChangedMessage message)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
            {
                BackgroundOpacity = message.TerminalOptions.BackgroundOpacity;
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

        private Task ShowAbout()
        {
            return _dialogService.ShowAboutDialogAsync();
        }

        private void ShowSettings()
        {
            ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ToggleFullScreen()
        {
            ApplicationView.ToggleFullScreen();
        }

        public void OnWindowKeyDown(int key, bool control, bool alt, bool shift, bool meta)
        {
            var binding = _keyBindings.FirstOrDefault(b =>
                b.Key == key && b.Ctrl == control && b.Alt == alt && b.Shift == shift && b.Meta == meta);

            if (binding != null)
            {
                _keyboardCommandService.SendCommand(binding.Command);
            }
        }
    }
}