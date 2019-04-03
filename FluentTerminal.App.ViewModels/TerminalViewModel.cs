using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private readonly IKeyboardCommandService _keyboardCommandService;
        private bool _isSelected;
        private bool _hasNewOutput;
        private string _searchText;
        private bool _showSearchPanel;
        private TabTheme _tabTheme;
        private TerminalTheme _terminalTheme;
        private OverlayViewModel _overlay;
        private string _tabTitle;
        private string _shellTitle;
        private bool _hasCustomTitle;

        public TerminalViewModel(ISettingsService settingsService, ITrayProcessCommunicationService trayProcessCommunicationService, IDialogService dialogService,
            IKeyboardCommandService keyboardCommandService, ApplicationSettings applicationSettings, ShellProfile shellProfile,
            IApplicationView applicationView, IDispatcherTimer dispatcherTimer, IClipboardService clipboardService)
        {
            SettingsService = settingsService;
            SettingsService.CurrentThemeChanged += OnCurrentThemeChanged;
            SettingsService.TerminalOptionsChanged += OnTerminalOptionsChanged;
            SettingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;
            SettingsService.KeyBindingsChanged += OnKeyBindingsChanged;

            TrayProcessCommunicationService = trayProcessCommunicationService;

            DialogService = dialogService;
            _keyboardCommandService = keyboardCommandService;
            ApplicationSettings = applicationSettings;
            ApplicationView = applicationView;
            ClipboardService = clipboardService;

            ShellProfile = shellProfile;
            TerminalTheme = shellProfile.TerminalThemeId == Guid.Empty ? SettingsService.GetCurrentTheme() : SettingsService.GetTheme(shellProfile.TerminalThemeId);

            TabThemes = new ObservableCollection<TabTheme>(SettingsService.GetTabThemes());
            TabTheme = TabThemes.FirstOrDefault(t => t.Id == ShellProfile.TabThemeId);

            CloseCommand = new RelayCommand(async () => await TryClose().ConfigureAwait(false));
            FindNextCommand = new RelayCommand(FindNext);
            FindPreviousCommand = new RelayCommand(FindPrevious);
            CloseSearchPanelCommand = new RelayCommand(CloseSearchPanel);
            SelectTabThemeCommand = new RelayCommand<string>(SelectTabTheme);
            EditTitleCommand = new AsyncCommand(EditTitle);

            Terminal = new Terminal(TrayProcessCommunicationService);
            Terminal.KeyboardCommandReceived += Terminal_KeyboardCommandReceived;
            Terminal.OutputReceived += Terminal_OutputReceived;
            Terminal.SizeChanged += Terminal_SizeChanged;
            Terminal.TitleChanged += Terminal_TitleChanged;
            Terminal.Closed += Terminal_Closed;

            Overlay = new OverlayViewModel(dispatcherTimer);

        }

        public event EventHandler Activated;
        public event EventHandler Closed;
        public event EventHandler<string> FindNextRequested;
        public event EventHandler<string> FindPreviousRequested;
        public event EventHandler KeyBindingsChanged;
        public event EventHandler<TerminalOptions> OptionsChanged;
        public event EventHandler SearchStarted;
        public event EventHandler<TerminalTheme> ThemeChanged;
        public event EventHandler<string> ShellTitleChanged;
        public event EventHandler<string> CustomTitleChanged;

        public ApplicationSettings ApplicationSettings { get; private set; }

        public IApplicationView ApplicationView { get; }

        public TabTheme BackgroundTabTheme
        {
            // The effective background theme depends on whether it is selected (use the theme), or if it is inactive
            // (if we're set to underline inactive tabs, use the null theme).
            get => IsSelected || (!IsSelected && ApplicationSettings.InactiveTabColorMode == InactiveTabColorMode.Background) ?
                _tabTheme :
                SettingsService.GetTabThemes().FirstOrDefault(t => t.Color == null);
        }

        public IClipboardService ClipboardService { get; }

        public RelayCommand CloseCommand { get; }

        public RelayCommand CloseSearchPanelCommand { get; }

        public IDialogService DialogService { get; }

        public IAsyncCommand EditTitleCommand { get; }

        public RelayCommand FindNextCommand { get; }

        public RelayCommand FindPreviousCommand { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (Set(ref _isSelected, value))
                {
                    if (IsSelected)
                    {
                        HasNewOutput = false;
                    }
                    RaisePropertyChanged(nameof(IsUnderlined));
                    RaisePropertyChanged(nameof(BackgroundTabTheme));
                }
            }
        }

        public bool IsUnderlined => (IsSelected && ApplicationSettings.UnderlineSelectedTab) ||
            (!IsSelected && ApplicationSettings.InactiveTabColorMode == InactiveTabColorMode.Underlined && TabTheme.Color != null);

        public bool HasNewOutput
        {
            get => _hasNewOutput;
            set => Set(ref _hasNewOutput, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public RelayCommand<string> SelectTabThemeCommand { get; }

        public ISettingsService SettingsService { get; }

        public ShellProfile ShellProfile { get; }

        public bool ShowSearchPanel
        {
            get => _showSearchPanel;
            set => Set(ref _showSearchPanel, value);
        }

        public TabTheme TabTheme
        {
            get => _tabTheme;
            set
            {
                Set(ref _tabTheme, value);
                RaisePropertyChanged(nameof(IsUnderlined));
                RaisePropertyChanged(nameof(BackgroundTabTheme));
            }
        }

        public ObservableCollection<TabTheme> TabThemes { get; }

        public Terminal Terminal { get; private set; }

        public OverlayViewModel Overlay { get; private set; }

        public TerminalTheme TerminalTheme
        {
            get => _terminalTheme;
            set => Set(ref _terminalTheme, value);
        }

        public string TabTitle
        {
            get => _tabTitle;
            set
            {
                if (Set(ref _tabTitle, value))
                {
                    CustomTitleChanged?.Invoke(this, value);
                }
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

        public Task Close()
        {
            SettingsService.CurrentThemeChanged -= OnCurrentThemeChanged;
            SettingsService.TerminalOptionsChanged -= OnTerminalOptionsChanged;
            SettingsService.ApplicationSettingsChanged -= OnApplicationSettingsChanged;
            SettingsService.KeyBindingsChanged -= OnKeyBindingsChanged;
            return Terminal.Close();
        }

        public void CopyText(string text)
        {
            ClipboardService.SetText(text);
        }

        public async Task EditTitle()
        {
            var result = await DialogService.ShowInputDialogAsync("Edit Title");
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

        private void FindNext()
        {
            FindNextRequested?.Invoke(this, SearchText);
        }

        private void FindPrevious()
        {
            FindPreviousRequested?.Invoke(this, SearchText);
        }

        private async void OnApplicationSettingsChanged(object sender, ApplicationSettings e)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
            {
                ApplicationSettings = e;
                RaisePropertyChanged(nameof(IsUnderlined));
                RaisePropertyChanged(nameof(BackgroundTabTheme));
            });
        }

        private async void OnCurrentThemeChanged(object sender, Guid e)
        {
            await ApplicationView.RunOnDispatcherThread(() =>
            {
                // only change theme if not overwritten by profile
                if (ShellProfile.TerminalThemeId == Guid.Empty)
                {
                    var currentTheme = SettingsService.GetTheme(e);
                    TerminalTheme = currentTheme;
                    ThemeChanged?.Invoke(this, currentTheme);
                }
            });
        }

        private async void OnKeyBindingsChanged(object sender, EventArgs e)
        {
            await ApplicationView.RunOnDispatcherThread(() => KeyBindingsChanged?.Invoke(this, EventArgs.Empty));
        }

        private async void OnTerminalOptionsChanged(object sender, TerminalOptions e)
        {
            await ApplicationView.RunOnDispatcherThread(() => OptionsChanged?.Invoke(this, e));
        }

        private void SelectTabTheme(string name)
        {
            TabTheme = TabThemes.FirstOrDefault(t => t.Name == name);
        }

        private void Terminal_Closed(object sender, EventArgs e)
        {
            ApplicationView.RunOnDispatcherThread(() => Closed?.Invoke(this, EventArgs.Empty));
        }

        private async void Terminal_KeyboardCommandReceived(object sender, string e)
        {
            switch (e)
            {
                case nameof(Command.Copy):
                    {
                        var selection = await Terminal.GetSelectedText().ConfigureAwait(true);
                        ClipboardService.SetText(selection);
                        Overlay.Show("Text copied");
                        break;
                    }
                case nameof(Command.Paste):
                    {
                        string content = await ClipboardService.GetText().ConfigureAwait(true);
                        if (content != null)
                        {
                            content = ShellProfile.TranslateLineEndings(content);
                            await Terminal.Write(Encoding.UTF8.GetBytes(content)).ConfigureAwait(true);
                        }
                        break;
                    }
                case nameof(Command.PasteWithoutNewlines):
                    {
                        string content = await ClipboardService.GetText().ConfigureAwait(true);
                        if (content != null)
                        {
                            content = ShellProfile.NewlinePattern.Replace(content, string.Empty);
                            await Terminal.Write(Encoding.UTF8.GetBytes(content)).ConfigureAwait(true);
                        }
                        break;
                    }
                case nameof(Command.Search):
                    {
                        ShowSearchPanel = !ShowSearchPanel;
                        if (ShowSearchPanel)
                        {
                            SearchStarted?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    }
                default:
                    {
                        _keyboardCommandService.SendCommand(e);
                        break;
                    }
            }
        }

        private void Terminal_OutputReceived(object sender, byte[] e)
        {
            if (!IsSelected && ApplicationSettings.ShowNewOutputIndicator)
            {
                ApplicationView.RunOnDispatcherThread(() => HasNewOutput = true);
            }
        }

        private void Terminal_SizeChanged(object sender, TerminalSize e)
        {
            Overlay.Show($"{e.Columns} x {e.Rows}");
        }

        private void Terminal_TitleChanged(object sender, string e)
        {
            ShellTitle = e;
            ShellTitleChanged?.Invoke(this, e);
        }

        private async Task TryClose()
        {
            if (ApplicationSettings.ConfirmClosingTabs)
            {
                var result = await DialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to close this tab?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                if (result == DialogButton.Cancel)
                {
                    return;
                }
            }

            await Close().ConfigureAwait(true);
        }
    }
}