using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Core;
using FluentTerminal.Models.Messages;
using System.Windows.Input;
using FluentTerminal.App.ViewModels.Menu;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using System.Collections.Generic;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        #region Static

        private static readonly Regex SshTitleRx = new Regex(@"^\w+@\S+:", RegexOptions.Compiled);

        #endregion Static

        #region Serialize terminal state

        private class TerminalState
        {
            public bool HasCustomTitle { get; set; }
            public string ShellTitle { get; set; }
            public string TabTitle { get; set; }
            public TerminalTheme TerminalTheme { get; set; }
            public int TabThemeId { get; set; }
            public bool ShowSearchPanel { get; set; }
            public string SearchText { get; set; }
            public bool SearchMatchCase { get; set; }
            public bool SearchWholeWord { get; set; }
            public bool SearchWithRegex { get; set; }
            public string XtermBufferState { get; set; }
            public byte TerminalId { get; set; }
            public ShellProfile ShellProfile { get; set; }
        }

        public async Task<string> SerializeAsync()
        {
            TerminalState state = new TerminalState
            {
                HasCustomTitle = _hasCustomTitle,
                ShellTitle = ShellTitle,
                TabTitle = TabTitle,
                TerminalTheme = TerminalTheme,
                TabThemeId = TabTheme.Theme.Id,
                ShowSearchPanel = ShowSearchPanel,
                SearchText = SearchText,
                SearchMatchCase = SearchMatchCase,
                SearchWholeWord = SearchWholeWord,
                SearchWithRegex = SearchWithRegex,
                XtermBufferState = await SerializeXtermStateAsync().ConfigureAwait(false),
                TerminalId = Terminal.Id,
                ShellProfile = ShellProfile
            };

            return JsonConvert.SerializeObject(state);
        }

        public void Restore(string data)
        {
            TerminalState state = JsonConvert.DeserializeObject<TerminalState>(data);
            if (state != null)
            {
                _hasCustomTitle = state.HasCustomTitle;
                ShellTitle = state.ShellTitle;
                TabTitle = state.TabTitle;
                TerminalTheme = state.TerminalTheme;
                TabTheme = TabThemes.FirstOrDefault(t => t.Theme.Id == state.TabThemeId);
                ShowSearchPanel = state.ShowSearchPanel;
                SearchText = state.SearchText;
                SearchMatchCase = state.SearchMatchCase;
                SearchWholeWord = state.SearchWholeWord;
                SearchWithRegex = state.SearchWithRegex;
                XtermBufferState = state.XtermBufferState;
                _terminalId = state.TerminalId;
                ShellProfile = state.ShellProfile;

                TabTheme.IsSelected = true;
            }
        }

        #endregion Serialize terminal state

        private readonly IKeyboardCommandService _keyboardCommandService;
        private bool _isSelected;
        private bool _isHovered;
        private bool _hasNewOutput;
        private bool _hasExitedWithError;
        private string _searchText;
        private bool _showSearchPanel;
        private bool _searchMatchCase;
        private bool _searchWholeWord;
        private bool _searchWithRegex;
        private TabThemeViewModel _tabTheme;
        private TerminalTheme _terminalTheme;
        private TerminalOptions _terminalOptions;
        private string _tabTitle;
        private string _shellTitle;
        private bool _hasCustomTitle;
        private byte? _terminalId;
        private bool _hasSelection;
        private string _hoveredUri;
        private MenuViewModel _contextMenu;
        private MenuViewModel _tabContextMenu;
        private readonly TabThemeViewModel _transparentTabThemeViewModel;

        public TerminalViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService,
            IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, ShellProfile shellProfile,
            IApplicationView applicationView, IClipboardService clipboardService, string terminalState = null)
        {
            MessengerInstance.Register<ApplicationSettingsChangedMessage>(this, OnApplicationSettingsChanged);
            MessengerInstance.Register<CurrentThemeChangedMessage>(this, OnCurrentThemeChanged);
            MessengerInstance.Register<TerminalOptionsChangedMessage>(this, OnTerminalOptionsChanged);
            MessengerInstance.Register<KeyBindingsChangedMessage>(this, OnKeyBindingsChanged);

            SettingsService = settingsService;

            _terminalOptions = SettingsService.GetTerminalOptions();

            TrayProcessCommunicationService = trayProcessCommunicationService;

            DialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            ApplicationSettings = applicationSettings;
            ApplicationView = applicationView;
            ClipboardService = clipboardService;

            ShellProfile = shellProfile;
            TerminalTheme = shellProfile.TerminalThemeId == Guid.Empty ? SettingsService.GetCurrentTheme() : SettingsService.GetTheme(shellProfile.TerminalThemeId);

            TabThemes = new ObservableCollection<TabThemeViewModel>(SettingsService.GetTabThemes().Select(theme => new TabThemeViewModel(theme, this)));
            CloseCommand = new AsyncCommand(TryCloseAsync, CanExecuteCommand);
            CloseLeftTabsCommand = new RelayCommand(CloseLeftTabs, CanExecuteCommand);
            CloseRightTabsCommand = new RelayCommand(CloseRightTabs, CanExecuteCommand);
            CloseOtherTabsCommand = new RelayCommand(CloseOtherTabs, CanExecuteCommand);
            FindNextCommand = new RelayCommand(FindNext, CanExecuteCommand);
            FindPreviousCommand = new RelayCommand(FindPrevious, CanExecuteCommand);
            CloseSearchPanelCommand = new RelayCommand(CloseSearchPanel, CanExecuteCommand);
            EditTitleCommand = new AsyncCommand(EditTitleAsync, CanExecuteCommand);
            DuplicateTabCommand = new RelayCommand(DuplicateTab, CanExecuteCommand);
            ReconnectTabCommand = new AsyncCommand(ReconnectTabAsync, () => CanExecuteCommand() && HasExitedWithError);
            CopyCommand = new AsyncCommand(Copy, () => HasSelection);
            PasteCommand = new AsyncCommand(Paste);
            CopyLinkCommand = new AsyncCommand(() => CopyTextAsync(HoveredUri), () => !string.IsNullOrWhiteSpace(HoveredUri));
            ShowSearchPanelCommand = new RelayCommand(() => ShowSearchPanel = true, () => !ShowSearchPanel);

            if (!string.IsNullOrEmpty(terminalState))
            {
                Restore(terminalState);
            }
            else
            {
                var defaultTabTheme = TabThemes.FirstOrDefault(t => t.Theme.Id == ShellProfile.TabThemeId);
                defaultTabTheme.IsSelected = true;
            }

            _transparentTabThemeViewModel = TabThemes.FirstOrDefault(t => t.Theme.Id == 0);

            Terminal = new Terminal(TrayProcessCommunicationService, _terminalId);
            Terminal.KeyboardCommandReceived += Terminal_KeyboardCommandReceived;
            Terminal.OutputReceived += Terminal_OutputReceived;
            Terminal.SizeChanged += Terminal_SizeChanged;
            Terminal.TitleChanged += Terminal_TitleChanged;
            Terminal.Exited += Terminal_Exited;
            Terminal.Closed += Terminal_Closed;

            ContextMenu = BuidContextMenu();
            TabContextMenu = BuildTabContextMenu();
        }

        public MenuViewModel ContextMenu
        {
            get => _contextMenu;
            set => Set(ref _contextMenu, value);
        }

        public MenuViewModel TabContextMenu
        {
            get => _tabContextMenu;
            set => Set(ref _tabContextMenu, value);
        }

        private bool _disposalRequested;

        public void DisposalPrepare()
        {
            _disposalRequested = true;
            CloseCommand = null;
            TerminalView.DisposalPrepare();
            TerminalView = null;
            Terminal = null;
            Overlay = null;
        }

        public event EventHandler Activated;
        public event EventHandler Closed;
        public event EventHandler<SearchRequest> FindNextRequested;
        public event EventHandler<SearchRequest> FindPreviousRequested;
        public event EventHandler SearchStarted;
        public event EventHandler<TerminalTheme> ThemeChanged;
        public event EventHandler CloseLeftTabsRequested;
        public event EventHandler CloseRightTabsRequested;
        public event EventHandler CloseOtherTabsRequested;
        public event EventHandler DuplicateTabRequested;

        public ApplicationSettings ApplicationSettings { get; private set; }

        public IApplicationView ApplicationView { get; }

        public TabThemeViewModel BackgroundTabTheme
        {
            // The effective background theme depends on whether it is selected (use the theme), or if it is inactive
            // (if we're set to underline inactive tabs, use the null theme).
            get => IsSelected || (!IsSelected && ApplicationSettings.InactiveTabColorMode == InactiveTabColorMode.Background) ?
                TabTheme :
                _transparentTabThemeViewModel;
        }

        public IClipboardService ClipboardService { get; }

        public string XtermBufferState { get; private set; }

        public AsyncCommand CloseCommand { get; private set; }

        public RelayCommand CloseRightTabsCommand { get; }

        public RelayCommand CloseLeftTabsCommand { get; }

        public RelayCommand CloseOtherTabsCommand { get; }

        public RelayCommand CloseSearchPanelCommand { get; }

        public IDialogService DialogService { get; }

        public IAsyncCommand EditTitleCommand { get; }

        public RelayCommand FindNextCommand { get; }

        public RelayCommand FindPreviousCommand { get; }

        public RelayCommand DuplicateTabCommand { get; }

        public AsyncCommand ReconnectTabCommand { get; }

        public ICommand CopyLinkCommand { get; private set; }

        public ICommand CopyCommand { get; private set; }

        public ICommand PasteCommand { get; private set; }

        public ICommand ShowSearchPanelCommand { get; private set; }

        public double BackgroundOpacity => _terminalOptions?.BackgroundOpacity ?? 1.0;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (Set(ref _isSelected, value))
                {
                    if (IsSelected)
                    {
                        _keyboardCommandService.RegisterCommandHandler(nameof(Command.Search), () => ShowSearchPanel = !ShowSearchPanel);
                        HasNewOutput = false;
                    }
                    else
                    {
                        _keyboardCommandService.DeregisterCommandHandler(nameof(Command.Search));
                    }
                    RaisePropertyChanged(nameof(IsUnderlined));
                    RaisePropertyChanged(nameof(BackgroundTabTheme));
                    RaisePropertyChanged(nameof(ShowCloseButton));
                }
            }
        }

        public bool HasSelection
        {
            get => _hasSelection;
            set => Set(ref _hasSelection, value);
        }

        public string HoveredUri
        {
            get => _hoveredUri;
            set
            {
                if (Set(ref _hoveredUri, value))
                {
                    ContextMenu = BuidContextMenu();
                }
            }
        }

        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (Set(ref _isHovered, value))
                {
                    RaisePropertyChanged(nameof(ShowCloseButton));
                }
            }
        }

        public bool ShowCloseButton
        {
            get => IsHovered || IsSelected;
        }

        public bool IsUnderlined => (IsSelected && ApplicationSettings.UnderlineSelectedTab) ||
            (!IsSelected && ApplicationSettings.InactiveTabColorMode == InactiveTabColorMode.Underlined && TabTheme.Theme.Color != null);

        public bool HasNewOutput
        {
            get => _hasNewOutput;
            set => Set(ref _hasNewOutput, value);
        }

        public bool HasExitedWithError
        {
            get => _hasExitedWithError;
            set
            {
                if (Set(ref _hasExitedWithError, value) && value)
                {
                    HasNewOutput = false;
                }
                ReconnectTabCommand.RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public bool SearchMatchCase
        {
            get => _searchMatchCase;
            set => Set(ref _searchMatchCase, value);
        }

        public bool SearchWholeWord
        {
            get => _searchWholeWord;
            set => Set(ref _searchWholeWord, value);
        }

        public bool SearchWithRegex
        {
            get => _searchWithRegex;
            set => Set(ref _searchWithRegex, value);
        }

        public ISettingsService SettingsService { get; }

        public ShellProfile ShellProfile { get; private set; }

        public bool ShowSearchPanel
        {
            get => _showSearchPanel;
            set => Set(ref _showSearchPanel, value);
        }

        public TabThemeViewModel TabTheme
        {
            get => _tabTheme;
            set
            {
                Set(ref _tabTheme, value);
                RaisePropertyChanged(nameof(IsUnderlined));
                RaisePropertyChanged(nameof(BackgroundTabTheme));
            }
        }

        public ObservableCollection<TabThemeViewModel> TabThemes { get; }

        public Terminal Terminal { get; private set; }

        public OverlayViewModel Overlay { get; set; }

        public TerminalTheme TerminalTheme
        {
            get => _terminalTheme;
            set => Set(ref _terminalTheme, value);
        }

        public bool Initialized { get; set; }

        public string TabTitle
        {
            get => _tabTitle;
            set
            {
                var title = value?.Trim() ?? string.Empty;

                if (ShellProfile is SshProfile)
                {
                    // For SshProfile we are adjusting title.
                    if (title.Equals("ssh", StringComparison.OrdinalIgnoreCase) ||
                        title.EndsWith("\\ssh.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        title = $"[ssh] {I18N.Translate("Authenticate")}";
                    }
                    else if (title.Equals("mosh", StringComparison.OrdinalIgnoreCase) ||
                             title.EndsWith("\\mosh.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        title = $"[mosh] {I18N.Translate("Authenticate")}";
                    }
                    else if (SshTitleRx.IsMatch(title))
                    {
                        title = $"[ssh] {title}";
                    }
                }

                Set(ref _tabTitle, title);
            }
        }

        public string ShellTitle
        {
            get => _shellTitle;
            set
            {
                if (Set(ref _shellTitle, value) && !_hasCustomTitle)
                {
                    TabTitle = value;
                }
            }
        }

        public ITrayProcessCommunicationService TrayProcessCommunicationService { get; }

        public Task CloseAsync()
        {
            MessengerInstance.Unregister(this);

            return Terminal.CloseAsync();
        }

        public Task CopyTextAsync(string text)
        {
            return ApplicationView.ExecuteOnUiThreadAsync(() =>
            {
                ClipboardService.SetText(text);

                if (ApplicationSettings.ShowTextCopied)
                {
                    Overlay.Show(I18N.Translate("TextCopied"));
                }
            });
        }

        // Requires UI thread
        public async Task EditTitleAsync()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var result = await DialogService.ShowInputDialogAsync(I18N.Translate("EditTitleString"))
                .ConfigureAwait(true);
            if (result != null)
            {
                if (string.IsNullOrWhiteSpace(result))
                {
                    _hasCustomTitle = false;
                    TabTitle = ShellTitle;
                }
                else
                {
                    _hasCustomTitle = true;
                    TabTitle = result;
                }
            }
        }

        public void FocusTerminal()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        private void CloseSearchPanel()
        {
            SearchText = string.Empty;
            ShowSearchPanel = false;
            FocusTerminal();
        }

        private bool CanExecuteCommand<T>(T a)
        {
            return CanExecuteCommand();
        }

        private bool CanExecuteCommand()
        {
            return Initialized && !_disposalRequested;
        }

        private void CloseLeftTabs()
        {
            CloseLeftTabsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CloseRightTabs()
        {
            CloseRightTabsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CloseOtherTabs()
        {
            CloseOtherTabsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void FindNext()
        {
            FindNextRequested?.Invoke(this, new SearchRequest { MatchCase = SearchMatchCase, Regex = SearchWithRegex, Term = SearchText, WholeWord = SearchWholeWord });
        }

        public ITerminalView TerminalView { get; set; }

        private Task<string> SerializeXtermStateAsync()
        {
            return TerminalView?.SerializeXtermStateAsync() ?? Task.FromResult(string.Empty);
        }

        private void FindPrevious()
        {
            FindPreviousRequested?.Invoke(this, new SearchRequest { MatchCase = SearchMatchCase, Regex = SearchWithRegex, Term = SearchText, WholeWord = SearchWholeWord });
        }

        private void DuplicateTab()
        {
            DuplicateTabRequested?.Invoke(this, EventArgs.Empty);
        }

        public async Task ReconnectTabAsync()
        {
            HasExitedWithError = false;
            await TerminalView?.ReconnectAsync();
        }

        private void OnApplicationSettingsChanged(ApplicationSettingsChangedMessage message)
        {
            ApplicationSettings = message.ApplicationSettings;

            ApplicationView.ExecuteOnUiThreadAsync(() =>
            {
                RaisePropertyChanged(nameof(IsUnderlined));
                RaisePropertyChanged(nameof(BackgroundTabTheme));
            });
        }

        private void OnCurrentThemeChanged(CurrentThemeChangedMessage message)
        {
            // only change theme if not overwritten by profile
            if (ShellProfile.TerminalThemeId == Guid.Empty)
            {
                var currentTheme = SettingsService.GetTheme(message.ThemeId);

                ApplicationView.ExecuteOnUiThreadAsync(() =>
                {
                    TerminalTheme = currentTheme;
                    ThemeChanged?.Invoke(this, currentTheme);
                });
            }
        }

        private void OnTerminalOptionsChanged(TerminalOptionsChangedMessage message)
        {
            _terminalOptions = message.TerminalOptions;
            ApplicationView.ExecuteOnUiThreadAsync(() => RaisePropertyChanged(nameof(BackgroundOpacity)), CoreDispatcherPriority.Low);
        }

        private void OnKeyBindingsChanged(KeyBindingsChangedMessage message)
        {
            ApplicationView.ExecuteOnUiThreadAsync(() =>
            {
                var contextMenu = BuidContextMenu();
                if (!contextMenu.EquivalentTo(ContextMenu))
                {
                    ContextMenu = contextMenu;
                }

                var tabContextMenu = BuildTabContextMenu();
                if (!tabContextMenu.EquivalentTo(TabContextMenu))
                {
                    TabContextMenu = tabContextMenu;
                }

            }, CoreDispatcherPriority.Low, true);
        }

        private void Terminal_Exited(object sender, int exitCode)
        {
            if (ShellProfile?.Tag is ISessionSuccessTracker tracker && exitCode != 0)
            {
                tracker.SetInvalid();
            }

            ApplicationView.ExecuteOnUiThreadAsync(() => HasExitedWithError = exitCode > 0);
        }

        private void Terminal_Closed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
            Terminal.KeyboardCommandReceived -= Terminal_KeyboardCommandReceived;
            Terminal.OutputReceived -= Terminal_OutputReceived;
            Terminal.SizeChanged -= Terminal_SizeChanged;
            Terminal.TitleChanged -= Terminal_TitleChanged;
            Terminal.Exited -= Terminal_Exited;
            Terminal.Closed -= Terminal_Closed;
        }

        private async void Terminal_KeyboardCommandReceived(object sender, string e)
        {
            switch (e)
            {
                case nameof(Command.Copy):
                    {
                        await Copy().ConfigureAwait(false);
                        return;
                    }
                case nameof(Command.Paste):
                    {
                        await Paste().ConfigureAwait(false);
                        return;
                    }
                case nameof(Command.PasteWithoutNewlines):
                    {
                        string content = await ClipboardService.GetTextAsync().ConfigureAwait(false);
                        if (content != null)
                        {
                            content = ShellProfile.NewlinePattern.Replace(content, string.Empty);
                            TerminalView.Paste(content);
                        }
                        return;
                    }
                case nameof(Command.Search):
                    {
                        await ApplicationView.ExecuteOnUiThreadAsync(() =>
                        {
                            ShowSearchPanel = !ShowSearchPanel;
                            if (ShowSearchPanel)
                            {
                                SearchStarted?.Invoke(this, EventArgs.Empty);
                            }
                        }).ConfigureAwait(false);
                        return;
                    }
                default:
                    {
                        await ApplicationView.ExecuteOnUiThreadAsync(() => _keyboardCommandService.SendCommand(e))
                            .ConfigureAwait(false);
                        return;
                    }
            }
        }

        private void Terminal_OutputReceived(object sender, byte[] e)
        {
            if (ShellProfile?.Tag is ISessionSuccessTracker tracker)
            {
                tracker.SetOutputReceived();
            }

            if (!IsSelected && ApplicationSettings.ShowNewOutputIndicator)
            {
                ApplicationView.ExecuteOnUiThreadAsync(() => HasNewOutput = true);
            }
        }

        private void Terminal_SizeChanged(object sender, TerminalSize e)
        {
            ApplicationView.ExecuteOnUiThreadAsync(() => Overlay.Show($"{e.Columns} x {e.Rows}"));
        }

        private void Terminal_TitleChanged(object sender, string e)
        {
            ApplicationView.ExecuteOnUiThreadAsync(() => ShellTitle = e);
        }

        private async Task TryCloseAsync()
        {
            if (ShellProfile?.Tag is ISessionSuccessTracker tracker)
            {
                tracker.SetInvalid();
            }

            if (!ApplicationSettings.ConfirmClosingTabs || await DialogService
                    .ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                        string.Format(I18N.Translate("ConfirmCloseTab"), ShellTitle), DialogButton.OK,
                        DialogButton.Cancel).ConfigureAwait(false) == DialogButton.OK)
            {
                await CloseAsync().ConfigureAwait(false);
            }
        }

        private async Task Copy()
        {
            var selection = await Terminal.GetSelectedText().ConfigureAwait(false);
            await CopyTextAsync(selection).ConfigureAwait(false);
        }

        private async Task Paste()
        {
            var content = await ClipboardService.GetTextAsync().ConfigureAwait(false);
            if (content != null)
            {
                TerminalView.Paste(content);
            }
        }

        private MenuViewModel BuidContextMenu()
        {
            var commandKeyBindings = SettingsService.GetCommandKeyBindings();
            var contextMenu = new MenuViewModel();

            if (!string.IsNullOrWhiteSpace(HoveredUri))
            {
                var copyLink = new MenuItemViewModel(I18N.Translate("CopyLink"), CopyLinkCommand, icon: Mdl2Icon.Link());
                contextMenu.Items.Add(copyLink);
                contextMenu.Items.Add(new SeparatorMenuItemViewModel());
            }

            var copy = new MenuItemViewModel(I18N.Translate("Command.Copy"), CopyCommand, icon: Mdl2Icon.Copy());
            AddKeyBindings(copy, Command.Copy, commandKeyBindings);
            contextMenu.Items.Add(copy);

            var paste = new MenuItemViewModel(I18N.Translate("Command.Paste"), PasteCommand, icon: Mdl2Icon.Paste());
            AddKeyBindings(paste, Command.Paste, commandKeyBindings);
            contextMenu.Items.Add(paste);

            contextMenu.Items.Add(new SeparatorMenuItemViewModel());

            var editTitle = new MenuItemViewModel(I18N.Translate("EditTitle.Text"), EditTitleCommand, icon: Mdl2Icon.Edit());
            AddKeyBindings(editTitle, Command.ChangeTabTitle, commandKeyBindings);
            contextMenu.Items.Add(editTitle);

            var search = new MenuItemViewModel(I18N.Translate("Search.Text"), ShowSearchPanelCommand, icon: Mdl2Icon.Search());
            AddKeyBindings(search, Command.Search, commandKeyBindings);
            contextMenu.Items.Add(search);

            contextMenu.Items.Add(new SeparatorMenuItemViewModel());

            var duplicate = new MenuItemViewModel(I18N.Translate("Command.DuplicateTab"), DuplicateTabCommand);
            AddKeyBindings(duplicate, Command.DuplicateTab, commandKeyBindings);
            contextMenu.Items.Add(duplicate);

            contextMenu.Items.Add(new SeparatorMenuItemViewModel());

            var close = new MenuItemViewModel(I18N.Translate("Close"), CloseCommand, icon: Mdl2Icon.Cancel());
            AddKeyBindings(close, Command.CloseTab, commandKeyBindings);
            contextMenu.Items.Add(close);

            return contextMenu;
        }

        private MenuViewModel BuildTabContextMenu()
        {
            var commandKeyBindings = SettingsService.GetCommandKeyBindings();
            var contextMenu = new MenuViewModel();

            RadioMenuItemViewModel createTabColorItem(TabThemeViewModel tabTheme)
            {
                //todo localise name
                var icon = tabTheme.Theme.Id == 0 ? Mdl2Icon.PaginationDotOutline10() : Mdl2Icon.PaginationDotSolid10(tabTheme.Theme.Color);
                return new RadioMenuItemViewModel(I18N.Translate($"TabTheme.{tabTheme.Theme.Name}"),  $"{Terminal.Id}_tabTheme", tabTheme, bindingPath: nameof(TabThemeViewModel.IsSelected) , icon: icon);
            }

            var tabColor = new ExpandableMenuItemViewModel(I18N.Translate("Color.Text"));

            foreach (var tabTheme in TabThemes)
            {
                tabColor.SubItems.Add(createTabColorItem(tabTheme));
            }

            contextMenu.Items.Add(tabColor);

            var editTitle = new MenuItemViewModel(I18N.Translate("EditTitle.Text"), EditTitleCommand, icon: Mdl2Icon.Edit());
            AddKeyBindings(editTitle, Command.ChangeTabTitle, commandKeyBindings);
            contextMenu.Items.Add(editTitle);

            contextMenu.Items.Add(new SeparatorMenuItemViewModel());

            var duplicate = new MenuItemViewModel(I18N.Translate("Command.DuplicateTab"), DuplicateTabCommand);
            AddKeyBindings(duplicate, Command.DuplicateTab, commandKeyBindings);
            contextMenu.Items.Add(duplicate);

            contextMenu.Items.Add(new SeparatorMenuItemViewModel());

            var closeLeft = new MenuItemViewModel(I18N.Translate("CloseLeft.Text"), CloseLeftTabsCommand);
            contextMenu.Items.Add(closeLeft);

            var closeRight = new MenuItemViewModel(I18N.Translate("CloseRight.Text"), CloseRightTabsCommand);
            contextMenu.Items.Add(closeRight);

            var closeOther = new MenuItemViewModel(I18N.Translate("CloseOther.Text"), CloseOtherTabsCommand);
            contextMenu.Items.Add(closeOther);

            var close = new MenuItemViewModel(I18N.Translate("Close"), CloseCommand, icon: Mdl2Icon.Cancel());
            AddKeyBindings(close, Command.CloseTab, commandKeyBindings);
            contextMenu.Items.Add(close);

            return contextMenu;
        }

        private void AddKeyBindings(MenuItemViewModel menuItem, Command command, IDictionary<string, ICollection<KeyBinding>> commandKeyBindings)
        {
            var keyBindings = commandKeyBindings[command.ToString()];
            if (keyBindings?.Any() == true)
            {
                menuItem.KeyBinding = new MenuItemKeyBindingViewModel(keyBindings.First());
            }
        }
    }
}