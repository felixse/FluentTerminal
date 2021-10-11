using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.App.ViewModels.Menu;
using FluentTerminal.Models.Messages;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentTerminal.App.ViewModels
{
    public class MainViewModel : ObservableObject,
        IRecipient<ApplicationSettingsChangedMessage>,
        IRecipient<ShellProfileAddedMessage>,
        IRecipient<ShellProfileDeletedMessage>,
        IRecipient<ShellProfileChangedMessage>,
        IRecipient<DefaultShellProfileChangedMessage>,
        IRecipient<TerminalOptionsChangedMessage>,
        IRecipient<CommandHistoryChangedMessage>,
        IRecipient<KeyBindingsChangedMessage>
    {
        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly ICommandHistoryService _commandHistoryService;
        private ApplicationSettings _applicationSettings;
        private bool _useAcrylicBackground;
        private double _backgroundOpacity;
        private TerminalViewModel _selectedTerminal;
        private TabsPosition _tabsPosition;
        private string _windowTitle;
        private IDictionary<string, ICollection<KeyBinding>> _keyBindings;

        public MainViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService, IKeyboardCommandService keyboardCommandService, 
            IClipboardService clipboardService, ICommandHistoryService commandHistoryService)
        {
            _settingsService = settingsService;

            _trayProcessCommunicationService = trayProcessCommunicationService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _keyboardCommandService = keyboardCommandService;
            _commandHistoryService = commandHistoryService;

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewTab), async () => await AddDefaultProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewWindow), async () => await AddDefaultProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewSshTab), async () => await AddSshProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewSshWindow), async () => await AddSshProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewCustomCommandTab), async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Tab));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.NewCustomCommandWindow), async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Window));

            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ChangeTabTitle), async () => await SelectedTerminal.EditTitleAsync());
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.CloseTab), CloseCurrentTab);
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.DuplicateTab), () => AddTab(SelectedTerminal.ShellProfile.Clone()));
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.ReconnectTab), async () => { if (SelectedTerminal.ReconnectTabCommand.CanExecute(null)) await SelectedTerminal.ReconnectTabAsync(); });

            // empty command handlers for copy and paste so that the main window does not execute these when copy keyboard shortcut is used in a popup search panel 
            // in such a case an exception would be thrown if no handlers are available so these empty ones mitigate that. Also there is no need for these on the main window anyway
            // so leaving them empty is fine.
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.Paste), () => { });
            _keyboardCommandService.RegisterCommandHandler(nameof(Command.Copy), () => { });

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
            UseAcrylicBackground = options.UseAcrylicBackground;
            BackgroundOpacity = options.BackgroundOpacity;
            _applicationSettings = _settingsService.GetApplicationSettings();
            TabsPosition = _applicationSettings.TabsPosition;

            AddDefaultTabCommand = new RelayCommand(async () => await AddDefaultProfileAsync(NewTerminalLocation.Tab));

            //ApplicationView.CloseRequested += OnCloseRequest;
            //ApplicationView.Closed += OnClosed;
            Terminals.CollectionChanged += OnTerminalsCollectionChanged;

            LoadKeyBindings();

            _newDefaultTabCommand = new RelayCommand(async () => await AddDefaultProfileAsync(NewTerminalLocation.Tab));
            _newDefaultWindowCommand = new RelayCommand(async () => await AddDefaultProfileAsync(NewTerminalLocation.Window));
            _newRemoteTabCommand = new RelayCommand(async () => await AddSshProfileAsync(NewTerminalLocation.Tab));
            _newRemoteWindowCommand = new RelayCommand(async () => await AddSshProfileAsync(NewTerminalLocation.Window));
            _newQuickLaunchTabCommand = new RelayCommand(async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Tab));
            _newQuickLaunchWindowCommand = new RelayCommand(async () => await AddQuickLaunchProfileAsync(NewTerminalLocation.Window));

            _settingsCommand = new RelayCommand(ShowSettings);
            _aboutCommand = new AsyncRelayCommand(_dialogService.ShowAboutDialogAsync);
            _quitCommand = new AsyncRelayCommand(_trayProcessCommunicationService.QuitApplicationAsync);

            _defaultProfile = _settingsService.GetDefaultShellProfile();

            CreateMenuViewModel();

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private ShellProfile _defaultProfile;

        private void UpdateDefaultShellProfile()
        {
            var defaultProfile = _settingsService.GetDefaultShellProfile();

            // We need to rebuild the menu only if default profile Id or Name is changed
            var changeMenu = _defaultProfile == null || !_defaultProfile.Id.Equals(defaultProfile.Id) ||
                             !string.Equals(_defaultProfile.Name, defaultProfile.Name, StringComparison.Ordinal);

            _defaultProfile = defaultProfile;

            if (changeMenu)
            {
                //ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
            }
        }

        private void LoadKeyBindings() => _keyBindings = _settingsService.GetCommandKeyBindings();

        private void OnClosed(object sender, EventArgs e)
        {
            //MessengerInstance.Unregister(this); todo do we still need this?

            //ApplicationView.CloseRequested -= OnCloseRequest;
            //ApplicationView.Closed -= OnClosed;
            Terminals.CollectionChanged -= OnTerminalsCollectionChanged;

            _keyboardCommandService.ClearCommandHandlers();

            _applicationSettings = null;

            AddDefaultTabCommand = null;

            Closed?.Invoke(this, e);
        }

        private void OnTerminalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ShowTabsOnTop));
            OnPropertyChanged(nameof(ShowTabsOnBottom));
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

        public ICommand AddDefaultTabCommand { get; private set; }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (SetProperty(ref _windowTitle, value))
                {
                    //ApplicationView.Title = value; todo
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

        public bool UseAcrylicBackground
        {
            get => _useAcrylicBackground;
            set => SetProperty(ref _useAcrylicBackground, value);
        }

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => SetProperty(ref _backgroundOpacity, value);
        }

        public TerminalViewModel SelectedTerminal
        {
            get => _selectedTerminal;
            set
            {
                var oldValue = _selectedTerminal;

                if (SetProperty(ref _selectedTerminal, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.IsSelected = false;
                    }

                    if (_selectedTerminal != null)
                    {
                        _selectedTerminal.IsSelected = true;
                        _selectedTerminal.FocusTerminal();

                        SetWindowTitle(_selectedTerminal);
                    }
                }
            }
        }

        public TabsPosition TabsPosition
        {
            get => _tabsPosition;
            set => SetProperty(ref _tabsPosition, value);
        }

        public ObservableCollection<TerminalViewModel> Terminals { get; } = new ObservableCollection<TerminalViewModel>();

        #region Launching terminal sessions

        public Task AddDefaultProfileAsync(NewTerminalLocation location)
        {
            var profile = _settingsService.GetDefaultShellProfile();

            return AddProfileAsync(profile, location);
        }

        private async Task AddSshProfileAsync(NewTerminalLocation location)
        {
            var profile = await _dialogService.ShowSshConnectionInfoDialogAsync().ConfigureAwait(true);

            await AddProfileAsync(profile, location).ConfigureAwait(false);
        }

        private async Task AddQuickLaunchProfileAsync(NewTerminalLocation location)
        {
            var profile = await _dialogService.ShowCustomCommandDialogAsync().ConfigureAwait(true);

            await AddProfileAsync(profile, location).ConfigureAwait(false);
        }

        public Task AddProfileByGuidAsync(Guid profileId) =>
            AddProfileByGuidAsync(profileId, _applicationSettings.NewTerminalLocation);

        private async Task AddProfileByGuidAsync(Guid profileId, NewTerminalLocation location)
        {
            var profile = _settingsService.GetShellProfile(profileId) ?? _settingsService.GetSshProfile(profileId);

            await AddProfileAsync(profile, location).ConfigureAwait(false);
        }

        // For serialization
        public void AddTab(string terminalState, int position)
        {
            AddTab(new ShellProfile(), terminalState, position);
        }

        private Task AddProfileAsync(ShellProfile profile) =>
            AddProfileAsync(profile, _applicationSettings.NewTerminalLocation);

        private Task AddProfileAsync(ShellProfile profile, NewTerminalLocation location)
        {
            if (profile == null)
            {
                return Task.CompletedTask;
            }

            if (location == NewTerminalLocation.Tab)
            {
                AddTab(profile);
            }

            NewWindowRequested?.Invoke(this, new NewWindowRequestedEventArgs(profile));

            return Task.CompletedTask;
        }

        public void AddTab(ShellProfile profile) => AddTab(profile, string.Empty, Terminals.Count);

        private void AddTab(ShellProfile profile, string terminalState, int position)
        {
            profile.Tag = new DelayedHistorySaver(() => _commandHistoryService.MarkUsed(profile));

            var terminal = new TerminalViewModel(_settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService,
                _applicationSettings, profile, _clipboardService, terminalState);

            terminal.PropertyChanged += Terminal_PropertyChanged;
            terminal.Closed += OnTerminalClosed;
            terminal.CloseLeftTabsRequested += Terminal_CloseLeftTabsRequested;
            terminal.CloseRightTabsRequested += Terminal_CloseRightTabsRequested;
            terminal.CloseOtherTabsRequested += Terminal_CloseOtherTabsRequested;
            terminal.DuplicateTabRequested += Terminal_DuplicateTabRequested;
            Terminals.Insert(Math.Min(position, Terminals.Count), terminal);

            SelectedTerminal = terminal;


            //return ApplicationView.ExecuteOnUiThreadAsync(() =>
            //{

            //});
        }

        #endregion Launching terminal sessions

        private void Terminal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is TerminalViewModel terminalViewModel))
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(TerminalViewModel.ShellTitle):
                case nameof(TerminalViewModel.TabTitle):

                    SetWindowTitle(terminalViewModel);

                    break;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private Task SetWindowTitle(TerminalViewModel terminalViewModel)
        {
            return Task.CompletedTask;
            //return ApplicationView.ExecuteOnUiThreadAsync(() =>
            //{
            //    WindowTitle = !_applicationSettings.ShowCustomTitleInTitlebar ||
            //                  string.IsNullOrEmpty(terminalViewModel?.TabTitle)
            //        ? terminalViewModel?.ShellTitle
            //        : terminalViewModel.TabTitle;
            //}, CoreDispatcherPriority.Low);
        }

        private void Terminal_DuplicateTabRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                AddTab(terminal.ShellProfile.Clone());
            }
        }

        private async void Terminal_CloseOtherTabsRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                await Task.WhenAll(Terminals.Where(t => t != terminal).Select(t =>
                {
                    Logger.Instance.Debug("Terminal with Id: {@id} closed.", t.Terminal.Id);
                    return t.CloseAsync();
                })).ConfigureAwait(false);
            }
        }

        private async void Terminal_CloseRightTabsRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                var toRemove = Terminals.Skip(Terminals.IndexOf(terminal) + 1).ToList();

                await Task.WhenAll(toRemove.Select(t =>
                {
                    Logger.Instance.Debug("Terminal with Id: {@id} closed.", t.Terminal.Id);
                    return t.CloseAsync();
                })).ConfigureAwait(false);
            }
        }

        private async void Terminal_CloseLeftTabsRequested(object sender, EventArgs e)
        {
            if (sender is TerminalViewModel terminal)
            {
                var toRemove = Terminals.Take(Terminals.IndexOf(terminal)).ToList();

                await Task.WhenAll(toRemove.Select(t =>
                {
                    Logger.Instance.Debug("Terminal with Id: {@id} closed.", t.Terminal.Id);
                    return t.CloseAsync();
                })).ConfigureAwait(false);
            }
        }

        private Task CloseAllTerminalsAsync()
        {
            return Task.WhenAll(Terminals.Select(terminal =>
            {
                terminal.PropertyChanged -= Terminal_PropertyChanged;
                terminal.Closed -= OnTerminalClosed;
                terminal.CloseLeftTabsRequested -= Terminal_CloseLeftTabsRequested;
                terminal.CloseRightTabsRequested -= Terminal_CloseRightTabsRequested;
                terminal.CloseOtherTabsRequested -= Terminal_CloseOtherTabsRequested;
                terminal.DuplicateTabRequested -= Terminal_DuplicateTabRequested;
                return terminal.CloseAsync();
            }));
        }

        private void CloseCurrentTab()
        {
            SelectedTerminal?.CloseAsync();
        }

        private async Task OnCloseRequest(object sender, CancelableEventArgs e)
        {
            if (_applicationSettings.ConfirmClosingWindows)
            {
                var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                    I18N.Translate("ConfirmCloseWindow"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(false);

                if (result == DialogButton.OK)
                {
                    await CloseAllTerminalsAsync().ConfigureAwait(false);
                }
                else
                {
                    e.Cancelled = true;
                }
            }
            else
            {
                await CloseAllTerminalsAsync().ConfigureAwait(false);
            }
        }

        private void OnTerminalClosed(object sender, EventArgs e)
        {
            if (!(sender is TerminalViewModel terminal))
            {
                return;
            }

            Logger.Instance.Debug("Terminal with Id: {@id} closed.", terminal.Terminal.Id);

            terminal.PropertyChanged -= Terminal_PropertyChanged;
            terminal.Closed -= OnTerminalClosed;
            terminal.CloseLeftTabsRequested -= Terminal_CloseLeftTabsRequested;
            terminal.CloseRightTabsRequested -= Terminal_CloseRightTabsRequested;
            terminal.CloseOtherTabsRequested -= Terminal_CloseOtherTabsRequested;
            terminal.DuplicateTabRequested -= Terminal_DuplicateTabRequested;

            //ApplicationView.ExecuteOnUiThreadAsync(() =>
            //{
            //    var wasSelected = _selectedTerminal == terminal;

            //    Terminals.Remove(terminal);

            //    if (Terminals.Count == 0)
            //    {
            //        ApplicationView.TryCloseAsync();
            //    }
            //    else if (wasSelected)
            //    {
            //        SelectedTerminal = Terminals.LastOrDefault(t => t != terminal);
            //    }
            //});
        }

        public void Receive(ShellProfileDeletedMessage message)
        {
            _keyboardCommandService.DeregisterCommandHandler(message.ProfileId.ToString());

            UpdateDefaultShellProfile();

            //ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
        }

        public void Receive(ShellProfileChangedMessage message)
        {
            UpdateDefaultShellProfile();

            //ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
        }

        public void Receive(ShellProfileAddedMessage message)
        {
            _keyboardCommandService.RegisterCommandHandler(message.ShellProfile.Id.ToString(),
                async () => await AddProfileByGuidAsync(message.ShellProfile.Id));

            UpdateDefaultShellProfile();

            //ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
        }

        public void Receive(DefaultShellProfileChangedMessage message)
        {
            UpdateDefaultShellProfile();
        }

        public void Receive(ApplicationSettingsChangedMessage message)
        {
            var updateMenu = _applicationSettings != null &&
                             (_applicationSettings.TabWindowCascadingAppMenu !=
                              message.ApplicationSettings.TabWindowCascadingAppMenu ||
                              !message.ApplicationSettings.TabWindowCascadingAppMenu &&
                              _applicationSettings.NewTerminalLocation !=
                              message.ApplicationSettings.NewTerminalLocation);

            _applicationSettings = message.ApplicationSettings;

            SetWindowTitle(SelectedTerminal);

            //ApplicationView.ExecuteOnUiThreadAsync(() =>
            //{
            //    TabsPosition = message.ApplicationSettings.TabsPosition;
            //    OnPropertyChanged(nameof(ShowTabsOnTop));
            //    OnPropertyChanged(nameof(ShowTabsOnBottom));
            //});

            //if (updateMenu)
            //{
            //    ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
            //}
        }

        public void Receive(TerminalOptionsChangedMessage message)
        {
            //ApplicationView.ExecuteOnUiThreadAsync(() =>
            //{
            //    BackgroundOpacity = message.TerminalOptions.BackgroundOpacity;
            //    UseAcrylicBackground = message.TerminalOptions.UseAcrylicBackground;
            //}, CoreDispatcherPriority.Low);
        }

        public void Receive(KeyBindingsChangedMessage message)
        {
            LoadKeyBindings();

            //ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
        }

        public void Receive(CommandHistoryChangedMessage message)
        {
            //ApplicationView.ExecuteOnUiThreadAsync(CreateMenuViewModel, CoreDispatcherPriority.Low, true);
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
            //ApplicationView.ToggleFullScreen(); todo
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

        private MenuViewModel _menuViewModel;

        private readonly ICommand _newDefaultTabCommand;
        private readonly ICommand _newDefaultWindowCommand;
        private readonly ICommand _newRemoteTabCommand;
        private readonly ICommand _newRemoteWindowCommand;
        private readonly ICommand _newQuickLaunchTabCommand;
        private readonly ICommand _newQuickLaunchWindowCommand;
        private readonly ICommand _settingsCommand;
        private readonly ICommand _aboutCommand;
        private readonly ICommand _quitCommand;

        public MenuViewModel AppMenuViewModel
        {
            get => _menuViewModel;
            private set => SetProperty(ref _menuViewModel, value);
        }

        private void CreateMenuViewModel()
        {
            var appMenuViewModel = new MenuViewModel();

            //if (_applicationSettings.TabWindowCascadingAppMenu)
            //{
            //    var tabItem = new ExpandableMenuItemViewModel(
            //        I18N.TranslateWithFallback("MenuItem_NewTab_Text", "New Tab"),
            //        description: I18N.TranslateWithFallback("MenuItem_NewTab_Text", "Launches a session in a new tab."),
            //        icon: Mdl2Icon.Add());

            //    FillCoreItems(tabItem.SubItems, NewTerminalLocation.Tab);

            //    appMenuViewModel.Items.Add(tabItem);

            //    var windowItem = new ExpandableMenuItemViewModel(
            //        I18N.TranslateWithFallback("MenuItem_NewWindow_Text", "New Window"),
            //        description: I18N.TranslateWithFallback("MenuItem_NewWindow_Description", "Launches a session in a new window."),
            //        icon: Mdl2Icon.NewWindow());

            //    FillCoreItems(windowItem.SubItems, NewTerminalLocation.Window);

            //    appMenuViewModel.Items.Add(windowItem);
            //}
            //else
            //{
            //    FillCoreItems(appMenuViewModel.Items, _applicationSettings.NewTerminalLocation);
            //}

            //appMenuViewModel.Items.Add(new ExpandableMenuItemViewModel(I18N.TranslateWithFallback("Recent.Text", "Recent"),
            //    GetRecentMenuItems(), I18N.TranslateWithFallback("Recent_Description", "Recently opened sessions."),
            //    icon: Mdl2Icon.History()));

            //appMenuViewModel.Items.Add(new SeparatorMenuItemViewModel());

            var settingsItem = new MenuItemViewModel(I18N.TranslateWithFallback("Settings.Text", "Settings"),
                _settingsCommand, I18N.TranslateWithFallback("Settings_Description", "Opens settings window."),
                icon: Mdl2Icon.Settings());

            if (_keyBindings.TryGetValue(nameof(Command.ShowSettings), out var keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding settingsKeyBinding)
            {
                //LoadKeyBindingsFromModel(settingsItem, settingsKeyBinding);
            }
            else
            {
                settingsItem.KeyBinding = null;
            }

            appMenuViewModel.Items.Add(settingsItem);

            appMenuViewModel.Items.Add(new MenuItemViewModel(I18N.TranslateWithFallback("AboutDialog.Title", "About"), _aboutCommand,
                I18N.TranslateWithFallback("About_Description", "Basic info about the app."),
                icon: Mdl2Icon.Info()));

            appMenuViewModel.Items.Add(new SeparatorMenuItemViewModel());

            var quitItem = new MenuItemViewModel(I18N.TranslateWithFallback("Quit.Text", "Quit"), _quitCommand,
                I18N.TranslateWithFallback("Quit.Description", "Quit Fluent Terminal"),
                icon: Mdl2Icon.Cancel());
            appMenuViewModel.Items.Add(quitItem);

            if (!appMenuViewModel.EquivalentTo(_menuViewModel))
            {
                AppMenuViewModel = appMenuViewModel;
            }
        }

        private void FillCoreItems(ObservableCollection<MenuItemViewModelBase> items, NewTerminalLocation location)
        {
            var tab = location == NewTerminalLocation.Tab;

            if (_defaultProfile?.Name is string defaultProfileName)
            {
                var defaultProfileItem = new MenuItemViewModel(
                    defaultProfileName, tab ? _newDefaultTabCommand : _newDefaultWindowCommand,
                    I18N.TranslateWithFallback("MenuItem_DefaultProfile_Description",
                        "Starts new terminal session based on the default profile."), icon: Mdl2Icon.FavoriteStar());

                var defaultProfileCommand = tab ? nameof(Command.NewTab) : nameof(Command.NewWindow);

                if (_keyBindings.TryGetValue(defaultProfileCommand, out var kbs) &&
                    kbs.FirstOrDefault() is KeyBinding tabKeyBindings)
                {
                    LoadKeyBindingsFromModel(defaultProfileItem, tabKeyBindings);
                }
                else
                {
                    defaultProfileItem.KeyBinding = null;
                }

                items.Add(defaultProfileItem);
            }

            var remoteConnectItem = new MenuItemViewModel(
                I18N.TranslateWithFallback("MenuItem_Remote_Text", "Remote Connect..."),
                tab ? _newRemoteTabCommand : _newRemoteWindowCommand,
                I18N.TranslateWithFallback("MenuItem_Remote_Description",
                    "Opens a dialog for launching a new SSH or Mosh terminal session."),
                icon: Mdl2Icon.Globe());

            var command = tab ? nameof(Command.NewSshTab) : nameof(Command.NewSshWindow);

            if (_keyBindings.TryGetValue(command, out var keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding remoteTabKeyBinding)
            {
                LoadKeyBindingsFromModel(remoteConnectItem, remoteTabKeyBinding);
            }
            else
            {
                remoteConnectItem.KeyBinding = null;
            }

            items.Add(remoteConnectItem);

            var quickLaunchItem = new MenuItemViewModel(
                I18N.TranslateWithFallback("MenuItem_QuickLaunch_Text", "Quick Launch..."),
                tab ? _newQuickLaunchTabCommand : _newQuickLaunchWindowCommand,
                I18N.TranslateWithFallback("MenuItem_QuickLaunch_Description",
                    "Opens a \"Quick Launch\" dialog for starting a new terminal session."),
                icon: Mdl2Icon.Play());

            command = tab ? nameof(Command.NewCustomCommandTab) : nameof(Command.NewCustomCommandWindow);

            if (_keyBindings.TryGetValue(command, out keyBindings) &&
                keyBindings.FirstOrDefault() is KeyBinding quickTabKeyBinding)
            {
                LoadKeyBindingsFromModel(quickLaunchItem, quickTabKeyBinding);
            }
            else
            {
                quickLaunchItem.KeyBinding = null;
            }

            items.Add(quickLaunchItem);

            items.Add(new SeparatorMenuItemViewModel());

            foreach (var profile in _settingsService.GetShellProfiles().Concat(_settingsService.GetSshProfiles()).OrderBy(x => x.Name))
            {
                items.Add(new MenuItemViewModel(profile.Name, new AsyncRelayCommand(() => AddProfileAsync(profile, location))));
            }
        }

        private ObservableCollection<MenuItemViewModel> GetRecentMenuItems() =>
            new ObservableCollection<MenuItemViewModel>(_commandHistoryService
                .GetHistoryRecentFirst(top: RecentItemsMaxCount).Select(CommandToMenuItem));

        private MenuItemViewModel CommandToMenuItem(ExecutedCommand command)
        {
            var itemCommand = new AsyncRelayCommand(() => AddProfileAsync(command.ShellProfile));
            var keyBinding = command.ShellProfile.KeyBindings?.FirstOrDefault() is KeyBinding kb
                ? new MenuItemKeyBindingViewModel(kb)
                : null;

            return new MenuItemViewModel(command.Value, itemCommand, keyBinding: keyBinding);
        }

        private void LoadKeyBindingsFromModel(MenuItemViewModel menuItemViewModel, KeyBinding keyBinding)
        {
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