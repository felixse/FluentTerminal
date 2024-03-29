﻿using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
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
using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ObservableObject,
        IRecipient<ApplicationSettingsChangedMessage>,
        IRecipient<CurrentThemeChangedMessage>,
        IRecipient<TerminalOptionsChangedMessage>,
        IRecipient<KeyBindingsChangedMessage>
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
            public int FontSize { get; set; }
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
                ShellProfile = ShellProfile,
                FontSize = FontSize
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
                FontSize = state.FontSize;
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
        private int _fontSize;

        public TerminalViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService,
            IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, ShellProfile shellProfile,
            IApplicationView applicationView, IClipboardService clipboardService, string terminalState = null)
        {
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
            CloseCommand = new AsyncRelayCommand(TryCloseAsync, CanExecuteCommand);
            CloseLeftTabsCommand = new RelayCommand(CloseLeftTabs, CanExecuteCommand);
            CloseRightTabsCommand = new RelayCommand(CloseRightTabs, CanExecuteCommand);
            CloseOtherTabsCommand = new RelayCommand(CloseOtherTabs, CanExecuteCommand);
            FindNextCommand = new RelayCommand(FindNext, CanExecuteCommand);
            FindPreviousCommand = new RelayCommand(FindPrevious, CanExecuteCommand);
            CloseSearchPanelCommand = new RelayCommand(CloseSearchPanel, CanExecuteCommand);
            EditTitleCommand = new AsyncRelayCommand(EditTitleAsync, CanExecuteCommand);
            DuplicateTabCommand = new RelayCommand(DuplicateTab, CanExecuteCommand);
            ReconnectTabCommand = new AsyncRelayCommand(ReconnectTabAsync, () => CanExecuteCommand() && HasExitedWithError);
            CopyCommand = new AsyncRelayCommand(Copy, () => HasSelection);
            PasteCommand = new AsyncRelayCommand(Paste);
            CopyLinkCommand = new AsyncRelayCommand(() => CopyTextAsync(HoveredUri), () => !string.IsNullOrWhiteSpace(HoveredUri));
            ShowSearchPanelCommand = new RelayCommand(() => ShowSearchPanel = true, () => !ShowSearchPanel);

            FontSize = _terminalOptions.FontSize;

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

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public MenuViewModel ContextMenu
        {
            get => _contextMenu;
            set => SetProperty(ref _contextMenu, value);
        }

        public MenuViewModel TabContextMenu
        {
            get => _tabContextMenu;
            set => SetProperty(ref _tabContextMenu, value);
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
        public event EventHandler<int> FontSizeChanged;

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

        public ICommand CloseCommand { get; private set; }

        public ICommand CloseRightTabsCommand { get; }

        public ICommand CloseLeftTabsCommand { get; }

        public ICommand CloseOtherTabsCommand { get; }

        public ICommand CloseSearchPanelCommand { get; }

        public IDialogService DialogService { get; }

        public ICommand EditTitleCommand { get; }

        public ICommand FindNextCommand { get; }

        public ICommand FindPreviousCommand { get; }

        public ICommand DuplicateTabCommand { get; }

        public AsyncRelayCommand ReconnectTabCommand { get; }

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
                if (SetProperty(ref _isSelected, value))
                {
                    if (IsSelected)
                    {
                        _keyboardCommandService.RegisterCommandHandler(nameof(Command.Search), () => ShowSearchPanel = true);
                        _keyboardCommandService.RegisterCommandHandler(nameof(Command.CloseSearch), () => CloseSearchPanel());
                        HasNewOutput = false;
                    }
                    else
                    {
                        _keyboardCommandService.DeregisterCommandHandler(nameof(Command.Search));
                        _keyboardCommandService.DeregisterCommandHandler(nameof(Command.CloseSearch));
                    }
                    OnPropertyChanged(nameof(IsUnderlined));
                    OnPropertyChanged(nameof(BackgroundTabTheme));
                    OnPropertyChanged(nameof(ShowCloseButton));
                }
            }
        }

        public bool HasSelection
        {
            get => _hasSelection;
            set => SetProperty(ref _hasSelection, value);
        }

        public string HoveredUri
        {
            get => _hoveredUri;
            set
            {
                if (SetProperty(ref _hoveredUri, value))
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
                if (SetProperty(ref _isHovered, value))
                {
                    OnPropertyChanged(nameof(ShowCloseButton));
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
            set => SetProperty(ref _hasNewOutput, value);
        }

        public bool HasExitedWithError
        {
            get => _hasExitedWithError;
            set
            {
                if (SetProperty(ref _hasExitedWithError, value) && value)
                {
                    HasNewOutput = false;
                }
                ReconnectTabCommand.NotifyCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool SearchMatchCase
        {
            get => _searchMatchCase;
            set => SetProperty(ref _searchMatchCase, value);
        }

        public bool SearchWholeWord
        {
            get => _searchWholeWord;
            set => SetProperty(ref _searchWholeWord, value);
        }

        public bool SearchWithRegex
        {
            get => _searchWithRegex;
            set => SetProperty(ref _searchWithRegex, value);
        }

        public ISettingsService SettingsService { get; }

        public ShellProfile ShellProfile { get; private set; }

        public bool ShowSearchPanel
        {
            get => _showSearchPanel;
            set => SetProperty(ref _showSearchPanel, value);
        }

        public bool SearchHasFocus { get; set; }

        public TabThemeViewModel TabTheme
        {
            get => _tabTheme;
            set
            {
                SetProperty(ref _tabTheme, value);
                OnPropertyChanged(nameof(IsUnderlined));
                OnPropertyChanged(nameof(BackgroundTabTheme));

                // necessary to update this constantly so that duplicated tabs will also carry over the color
                ShellProfile.TabThemeId = value.Theme.Id;
            }
        }

        public ObservableCollection<TabThemeViewModel> TabThemes { get; }

        public Terminal Terminal { get; private set; }

        public OverlayViewModel Overlay { get; set; }

        public TerminalTheme TerminalTheme
        {
            get => _terminalTheme;
            set => SetProperty(ref _terminalTheme, value);
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

                SetProperty(ref _tabTitle, title);
                ShellProfile.Name = title;
            }
        }

        public string ShellTitle
        {
            get => _shellTitle;
            set
            {
                if (SetProperty(ref _shellTitle, value) && !_hasCustomTitle)
                {
                    TabTitle = value;
                }
            }
        }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value > 0 ? value : 1;
            }
        }

        public ITrayProcessCommunicationService TrayProcessCommunicationService { get; }

        public Task CloseAsync()
        {
            //MessengerInstance.Unregister(this); // todo necessary?

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

        public void Receive(ApplicationSettingsChangedMessage message)
        {
            ApplicationSettings = message.ApplicationSettings;

            ApplicationView.ExecuteOnUiThreadAsync(() =>
            {
                OnPropertyChanged(nameof(IsUnderlined));
                OnPropertyChanged(nameof(BackgroundTabTheme));
            });
        }

        public void Receive(CurrentThemeChangedMessage message)
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

        public void Receive(TerminalOptionsChangedMessage message)
        {
            _terminalOptions = message.TerminalOptions;
            ApplicationView.ExecuteOnUiThreadAsync(() => OnPropertyChanged(nameof(BackgroundOpacity)), CoreDispatcherPriority.Low);
        }

        public void Receive(KeyBindingsChangedMessage message)
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
                            ShowSearchPanel = true;
                            SearchStarted?.Invoke(this, EventArgs.Empty);
                        }).ConfigureAwait(false);
                        return;
                    }
                case nameof(Command.CloseSearch):
                    {
                        await ApplicationView.ExecuteOnUiThreadAsync(() =>
                        {
                            CloseSearchPanel();
                        }).ConfigureAwait(false);
                        return;
                    }
                case nameof(Command.IncreaseFontSize):
                    {
                        FontSize++;
                        FontSizeChanged?.Invoke(this, FontSize);
                        return;
                    }
                case nameof(Command.DecreaseFontSize):
                    {
                        if (FontSize > 2)
                        {
                            FontSize--;
                            FontSizeChanged?.Invoke(this, FontSize);
                        }

                        return;
                    }
                case nameof(Command.ResetFontSize):
                    {
                        FontSize = _terminalOptions.FontSize;
                        FontSizeChanged?.Invoke(this, FontSize);
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
            if (e == Terminal.Profile.Location) return;
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
            // prevent from pasting something into the terminal window when the actual command is executed
            // while being in the search box
            if (!SearchHasFocus)
            {
                var content = await ClipboardService.GetTextAsync().ConfigureAwait(false);
                if (content != null)
                {
                    TerminalView.Paste(content);
                }
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
                return new RadioMenuItemViewModel(I18N.Translate($"{tabTheme.Theme.Name}"),  $"{Terminal.Id}_tabTheme", tabTheme, bindingPath: nameof(TabThemeViewModel.IsSelected) , icon: icon);
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