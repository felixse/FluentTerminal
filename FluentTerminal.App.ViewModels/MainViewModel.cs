using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.App.ViewModels.Menu;
using FluentTerminal.Models.Messages;
using GalaSoft.MvvmLight.Command;

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
        private readonly ICommandHistoryService _commandHistoryService;
        private readonly IAcceleratorKeyValidator _acceleratorKeyValidator;
        private ApplicationSettings _applicationSettings;
        private string _background;
        private double _backgroundOpacity;
        private TerminalViewModel _selectedTerminal;
        private TabsPosition _tabsPosition;
        private string _windowTitle;
        private IDictionary<string, ICollection<KeyBinding>> _keyBindings;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService, 
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService, ICommandHistoryService commandHistoryService, IAcceleratorKeyValidator acceleratorKeyValidator)
        {
            MessengerInstance.Register<ApplicationSettingsChangedMessage>(this, OnApplicationSettingsChanged);
            MessengerInstance.Register<CurrentThemeChangedMessage>(this, OnCurrentThemeChanged);
            MessengerInstance.Register<ShellProfileAddedMessage>(this, OnShellProfileAdded);
            MessengerInstance.Register<ShellProfileDeletedMessage>(this, OnShellProfileDeleted);
            MessengerInstance.Register<SshProfileAddedMessage>(this, OnSshProfileAdded);
            MessengerInstance.Register<SshProfileDeletedMessage>(this, OnSshProfileDeleted);
            MessengerInstance.Register<TerminalOptionsChangedMessage>(this, OnTerminalOptionsChanged);
            MessengerInstance.Register<CommandHistoryChangedMessage>(this, OnCommandHistoryChanged);
            MessengerInstance.Register<KeyBindingsChangedMessage>(this, OnKeyBindingChanged);

            _settingsService = settingsService;

            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            ApplicationView = applicationView;
            _dispatcherTimer = dispatcherTimer;
            _clipboardService = clipboardService;
            _keyboardCommandService = keyboardCommandService;
            _commandHistoryService = commandHistoryService;
            _acceleratorKeyValidator = acceleratorKeyValidator;

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

            ApplicationView.CloseRequested += OnCloseRequest;
            ApplicationView.Closed += OnClosed;
            Terminals.CollectionChanged += OnTerminalsCollectionChanged;

            LoadKeyBindings();

            InitializeAppMenu();
        }

        private void LoadKeyBindings() => _keyBindings = _settingsService.GetCommandKeyBindings();

        private void OnClosed(object sender, EventArgs e)
        {
            MessengerInstance.Unregister(this);

            ApplicationView.CloseRequested -= OnCloseRequest;
            ApplicationView.Closed -= OnClosed;
            Terminals.CollectionChanged -= OnTerminalsCollectionChanged;

            _keyboardCommandService.ClearCommandHandlers();

            _applicationSettings = null;

            AddLocalShellCommand = null;

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

        private Task AddProfileAsync(ShellProfile profile) =>
            AddProfileAsync(profile, _settingsService.GetApplicationSettings().NewTerminalLocation);

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

        private void OnKeyBindingChanged(KeyBindingsChangedMessage message)
        {
            LoadKeyBindings();

            // Should be scheduled no matter if we're in the UI thread.
            ApplicationView.RunOnDispatcherThread(CreateMenuViewModel);
        }

        private void OnCommandHistoryChanged(CommandHistoryChangedMessage message)
        {
            // Should be scheduled no matter if we're in the UI thread.
            ApplicationView.RunOnDispatcherThread(CreateMenuViewModel);
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

        public void ShowSettings()
        {
            ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ToggleFullScreen()
        {
            ApplicationView.ToggleFullScreen();
        }

        public void OnWindowKeyDown(int key, bool control, bool alt, bool shift, bool meta)
        {
            var binding = _keyBindings.Values.SelectMany(bs => bs).FirstOrDefault(b =>
                b.Key == key && b.Ctrl == control && b.Alt == alt && b.Shift == shift && b.Meta == meta);

            if (binding != null)
            {
                _keyboardCommandService.SendCommand(binding.Command);
            }
        }

        #region App menu

        private const int RecentItemsMaxCount = 10;

        private AppMenuViewModel _menuViewModel;

        private RelayCommand _newRemoteTabCommand;
        private RelayCommand _newQuickTabCommand;
        private RelayCommand _settingsCommand;
        private RelayCommand _aboutCommand;

        public AppMenuViewModel MenuViewModel
        {
            get => _menuViewModel;
            private set
            {
                // Changing app menu view model only if it's actually different than previous.
                if (!value.EquivalentTo(_menuViewModel))
                {
                    Set(ref _menuViewModel, value);
                }
            }
        }

        private void InitializeAppMenu()
        {
            _newRemoteTabCommand = new RelayCommand(async () => await AddSshProfileAsync(NewTerminalLocation.Tab));
            _newQuickTabCommand = new RelayCommand(async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Tab));
            _settingsCommand = new RelayCommand(ShowSettings);
            _aboutCommand = new RelayCommand(async () => await _dialogService.ShowAboutDialogAsync());

            CreateMenuViewModel();
        }

        private void CreateMenuViewModel()
        {
            var tabItem = new MenuItemViewModel(I18N.TranslateWithFallback("NewTab.Text", "New tab"),
                AddLocalShellCommand,
                I18N.TranslateWithFallback("NewTab_Description", "Opens default profile in a new tab."),
                icon: 57609 /*(int) Symbol.Add*/);

            if (_keyBindings.TryGetValue(nameof(Command.NewTab), out var keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding tabKeyBindings &&
                _acceleratorKeyValidator.Valid(tabKeyBindings.Key))
            {
                LoadKeyBindingsFromModel(tabItem, tabKeyBindings);
            }
            else
            {
                tabItem.KeyBinding = null;
            }

            var remoteTabItem = new MenuItemViewModel(I18N.TranslateWithFallback("NewSshTab.Text", "New remote tab"),
                _newRemoteTabCommand,
                I18N.TranslateWithFallback("NewSshTab_Description", "Opens a new SSH or Mosh session in a new tab."),
                icon: 57609 /*(int) Symbol.Add*/);

            if (_keyBindings.TryGetValue(nameof(Command.NewSshTab), out keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding remoteTabKeyBinding &&
                _acceleratorKeyValidator.Valid(remoteTabKeyBinding.Key))
            {
                LoadKeyBindingsFromModel(remoteTabItem, remoteTabKeyBinding);
            }
            else
            {
                remoteTabItem.KeyBinding = null;
            }

            var quickTab = new MenuItemViewModel(I18N.TranslateWithFallback("NewQuickTab.Text", "New quick tab"),
                _newQuickTabCommand,
                I18N.TranslateWithFallback("NewQuickTab_Description",
                    "Opens \"Quick Launch\" dialog and starts session in a new tab."),
                icon: 57609 /*(int) Symbol.Add*/);

            if (_keyBindings.TryGetValue(nameof(Command.NewCustomCommandTab), out keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding quickTabKeyBinding &&
                _acceleratorKeyValidator.Valid(quickTabKeyBinding.Key))
            {
                LoadKeyBindingsFromModel(quickTab, quickTabKeyBinding);
            }
            else
            {
                quickTab.KeyBinding = null;
            }

            var settings = new MenuItemViewModel(I18N.TranslateWithFallback("Settings.Text", "Settings"),
                _settingsCommand, I18N.TranslateWithFallback("Settings_Description", "Opens settings window."),
                icon: 57621 /*(int) Symbol.Setting*/);

            if (_keyBindings.TryGetValue(nameof(Command.ShowSettings), out keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding settingsKeyBinding &&
                _acceleratorKeyValidator.Valid(settingsKeyBinding.Key))
            {
                LoadKeyBindingsFromModel(settings, settingsKeyBinding);
            }
            else
            {
                settings.KeyBinding = null;
            }

            var recent = new ExpandableMenuItemViewModel(I18N.TranslateWithFallback("Recent.Text", "Recent"),
                GetRecentMenuItems(), I18N.TranslateWithFallback("Recent_Description", "Recently opened sessions."),
                icon: "\uF738" /*Segoe MDL2 Assets Glyph property*/);

            var about = new MenuItemViewModel(I18N.TranslateWithFallback("AboutDialog.Title", "About"), _aboutCommand,
                I18N.TranslateWithFallback("About_Description", "Basic info about the app."),
                icon: "\uE946" /*Segoe MDL2 Assets Glyph property*/);

            var appMenuViewModel = new AppMenuViewModel(new MenuItemViewModelBase[]
            {
                tabItem,
                remoteTabItem,
                quickTab,
                settings,
                recent,
                about,
            });

            MenuViewModel = appMenuViewModel;
        }

        private ObservableCollection<MenuItemViewModel> GetRecentMenuItems() =>
            new ObservableCollection<MenuItemViewModel>(_commandHistoryService
                .GetHistoryRecentFirst(top: RecentItemsMaxCount).Select(CommandToMenuItem));

        private MenuItemViewModel CommandToMenuItem(ExecutedCommand command)
        {
            var itemCommand = new RelayCommand(async () => await AddProfileAsync(command.ShellProfile),
                keepTargetAlive: true);
            var keyBinding = command.ShellProfile.KeyBindings?.FirstOrDefault() is KeyBinding kb
                ? new MenuItemKeyBindingViewModel(kb)
                : null;

            return new MenuItemViewModel(command.Value, itemCommand, keyBinding: keyBinding);
        }

        private void LoadKeyBindingsFromModel(MenuItemViewModel menuItemViewModel, KeyBinding keyBinding)
        {
            if (!_acceleratorKeyValidator.Valid(keyBinding.Key))
            {
                menuItemViewModel.KeyBinding = null;

                return;
            }

            if (menuItemViewModel.KeyBinding == null)
            {
                menuItemViewModel.KeyBinding = new MenuItemKeyBindingViewModel(keyBinding);
            }
            else
            {
                menuItemViewModel.KeyBinding.Key = keyBinding.Key;
                menuItemViewModel.KeyBinding.Ctrl = keyBinding.Ctrl;
                menuItemViewModel.KeyBinding.Alt = keyBinding.Alt;
                menuItemViewModel.KeyBinding.Shift = keyBinding.Shift;
                menuItemViewModel.KeyBinding.Windows = keyBinding.Meta;
            }
        }

        #endregion App menu
    }
}