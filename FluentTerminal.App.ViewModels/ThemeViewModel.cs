using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class ThemeViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private string _author;
        private string _background;
        private double _backgroundOpacity;
        private string _black;
        private string _blue;
        private string _brightBlack;
        private string _brightBlue;
        private string _brightCyan;
        private string _brightGreen;
        private string _brightMagenta;
        private string _brightRed;
        private string _brightWhite;
        private string _brightYellow;
        private string _cursor;
        private string _cursorAccent;
        private string _cyan;
        private string _fallBackAuthor;
        private TerminalColors _fallBackColors;
        private string _fallBackName;
        private string _foreground;
        private string _green;
        private bool _inEditMode;
        private bool _isActive;
        private string _magenta;
        private string _name;
        private string _red;
        private string _selection;
        private readonly TerminalTheme _theme;
        private string _white;
        private string _yellow;
        private string _tabActiveBackground;
        private string _tabActiveUnderline;
        private string _tabActiveForeground;
        private string _tabInactiveBackground;
        private string _tabInactiveUnderline;
        private string _tabInactiveForeground;
        private readonly IFileSystemService _fileSystemService;

        public event EventHandler<string> BackgroundChanged;

        public ThemeViewModel(TerminalTheme theme, ISettingsService settingsService, IDialogService dialogService, IFileSystemService fileSystemService)
        {
            _theme = theme;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;

            Name = _theme.Name;
            Author = _theme.Author;
            Id = _theme.Id;

            Black = _theme.Colors.Black;
            Red = _theme.Colors.Red;
            Green = _theme.Colors.Green;
            Yellow = _theme.Colors.Yellow;
            Blue = _theme.Colors.Blue;
            Magenta = _theme.Colors.Magenta;
            Cyan = _theme.Colors.Cyan;
            White = _theme.Colors.White;

            BrightBlack = _theme.Colors.BrightBlack;
            BrightRed = _theme.Colors.BrightRed;
            BrightGreen = _theme.Colors.BrightGreen;
            BrightYellow = _theme.Colors.BrightYellow;
            BrightBlue = _theme.Colors.BrightBlue;
            BrightMagenta = _theme.Colors.BrightMagenta;
            BrightCyan = _theme.Colors.BrightCyan;
            BrightWhite = _theme.Colors.BrightWhite;

            Background = _theme.Colors.Background;
            Foreground = _theme.Colors.Foreground;
            Cursor = _theme.Colors.Cursor;
            CursorAccent = _theme.Colors.CursorAccent;
            Selection = _theme.Colors.Selection;

            TabActiveBackground = _theme.Colors.TabActiveBackground;
            TabActiveUnderline = _theme.Colors.TabActiveUnderline;
            TabActiveForeground = _theme.Colors.TabActiveForeground;
            TabInactiveBackground = _theme.Colors.TabInactiveBackground;
            TabInactiveUnderline = _theme.Colors.TabInactiveUnderline;
            TabInactiveForeground = _theme.Colors.TabInactiveForeground;

            SetActiveCommand = new RelayCommand(SetActive);
            DeleteCommand = new RelayCommand(async () => await Delete().ConfigureAwait(false), NotPreInstalled);
            EditCommand = new RelayCommand(Edit, NotPreInstalled);
            CancelEditCommand = new RelayCommand(async () => await CancelEdit().ConfigureAwait(false));
            SaveChangesCommand = new RelayCommand(SaveChanges);
            ExportCommand = new RelayCommand(async () => await Export().ConfigureAwait(false), NotPreInstalled);
        }

        public event EventHandler Activated;

        public event EventHandler Deleted;

        public string Author
        {
            get => _author;
            set => Set(ref _author, value);
        }

        public string Background
        {
            get => _background;
            set
            {
                Set(ref _background, value);
                BackgroundChanged?.Invoke(this, value);
            }
        }

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => Set(ref _backgroundOpacity, value);
        }

        public string Black
        {
            get => _black;
            set => Set(ref _black, value);
        }

        public string Blue
        {
            get => _blue;
            set => Set(ref _blue, value);
        }

        public string BrightBlack
        {
            get => _brightBlack;
            set => Set(ref _brightBlack, value);
        }

        public string BrightBlue
        {
            get => _brightBlue;
            set => Set(ref _brightBlue, value);
        }

        public string BrightCyan
        {
            get => _brightCyan;
            set => Set(ref _brightCyan, value);
        }

        public string BrightGreen
        {
            get => _brightGreen;
            set => Set(ref _brightGreen, value);
        }

        public string BrightMagenta
        {
            get => _brightMagenta;
            set => Set(ref _brightMagenta, value);
        }

        public string BrightRed
        {
            get => _brightRed;
            set => Set(ref _brightRed, value);
        }

        public string BrightWhite
        {
            get => _brightWhite;
            set => Set(ref _brightWhite, value);
        }

        public string BrightYellow
        {
            get => _brightYellow;
            set => Set(ref _brightYellow, value);
        }

        public RelayCommand CancelEditCommand { get; }

        public string Cursor
        {
            get => _cursor;
            set => Set(ref _cursor, value);
        }

        public string CursorAccent
        {
            get => _cursorAccent;
            set => Set(ref _cursorAccent, value);
        }

        public string Cyan
        {
            get => _cyan;
            set => Set(ref _cyan, value);
        }

        public RelayCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand ExportCommand { get; }

        public string Foreground
        {
            get => _foreground;
            set => Set(ref _foreground, value);
        }

        public string Green
        {
            get => _green;
            set => Set(ref _green, value);
        }

        public Guid Id { get; }

        public bool InEditMode
        {
            get => _inEditMode;
            set => Set(ref _inEditMode, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }

        public string Magenta
        {
            get => _magenta;
            set => Set(ref _magenta, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string Red
        {
            get => _red;
            set => Set(ref _red, value);
        }

        public RelayCommand SaveChangesCommand { get; }

        public string Selection
        {
            get => _selection;
            set => Set(ref _selection, value);
        }

        public RelayCommand SetActiveCommand { get; }

        public string White
        {
            get => _white;
            set => Set(ref _white, value);
        }

        public string Yellow
        {
            get => _yellow;
            set => Set(ref _yellow, value);
        }
        public string TabActiveBackground
        {
            get => _tabActiveBackground;
            set => Set(ref _tabActiveBackground, value);
        }

        public string TabActiveUnderline
        {
            get => _tabActiveUnderline;
            set => Set(ref _tabActiveUnderline, value);
        }

        public string TabActiveForeground
        {
            get => _tabActiveForeground;
            set => Set(ref _tabActiveForeground, value);
        }

        public string TabInactiveBackground
        {
            get => _tabInactiveBackground;
            set => Set(ref _tabInactiveBackground, value);
        }

        public string TabInactiveUnderline
        {
            get => _tabInactiveUnderline;
            set => Set(ref _tabInactiveUnderline, value);
        }

        public string TabInactiveForeground
        {
            get => _tabInactiveForeground;
            set => Set(ref _tabInactiveForeground, value);
        }

        public void SaveChanges()
        {
            _theme.Name = Name;
            _theme.Author = Author;

            _theme.Colors.Black = Black;
            _theme.Colors.Red = Red;
            _theme.Colors.Green = Green;
            _theme.Colors.Yellow = Yellow;
            _theme.Colors.Blue = Blue;
            _theme.Colors.Magenta = Magenta;
            _theme.Colors.Cyan = Cyan;
            _theme.Colors.White = White;

            _theme.Colors.BrightBlack = BrightBlack;
            _theme.Colors.BrightRed = BrightRed;
            _theme.Colors.BrightGreen = BrightGreen;
            _theme.Colors.BrightYellow = BrightYellow;
            _theme.Colors.BrightBlue = BrightBlue;
            _theme.Colors.BrightMagenta = BrightMagenta;
            _theme.Colors.BrightCyan = BrightCyan;
            _theme.Colors.BrightWhite = BrightWhite;

            _theme.Colors.Background = Background;
            _theme.Colors.Foreground = Foreground;
            _theme.Colors.Cursor = Cursor;
            _theme.Colors.CursorAccent = CursorAccent;
            _theme.Colors.Selection = Selection;

            _theme.Colors.TabActiveBackground = TabActiveBackground;
            _theme.Colors.TabActiveUnderline = TabActiveUnderline;
            _theme.Colors.TabActiveForeground = TabActiveForeground;
            _theme.Colors.TabInactiveBackground = TabInactiveBackground;
            _theme.Colors.TabInactiveUnderline = TabInactiveUnderline;
            _theme.Colors.TabInactiveForeground = TabInactiveForeground;

            _settingsService.SaveTheme(_theme);

            InEditMode = false;
        }

        private async Task CancelEdit()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to discard all changes?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Black = _fallBackColors.Black;
                Red = _fallBackColors.Red;
                Green = _fallBackColors.Green;
                Yellow = _fallBackColors.Yellow;
                Blue = _fallBackColors.Blue;
                Magenta = _fallBackColors.Magenta;
                Cyan = _fallBackColors.Cyan;
                White = _fallBackColors.White;

                BrightBlack = _fallBackColors.BrightBlack;
                BrightRed = _fallBackColors.BrightRed;
                BrightGreen = _fallBackColors.BrightGreen;
                BrightYellow = _fallBackColors.BrightYellow;
                BrightBlue = _fallBackColors.BrightBlue;
                BrightMagenta = _fallBackColors.BrightMagenta;
                BrightCyan = _fallBackColors.BrightCyan;
                BrightWhite = _fallBackColors.BrightWhite;

                Background = _fallBackColors.Background;
                Foreground = _fallBackColors.Foreground;
                Cursor = _fallBackColors.Cursor;
                CursorAccent = _fallBackColors.CursorAccent;
                Selection = _fallBackColors.Selection;

                TabActiveBackground = _fallBackColors.TabActiveBackground;
                TabActiveUnderline = _fallBackColors.TabActiveUnderline;
                TabActiveForeground = _fallBackColors.TabActiveForeground;
                TabInactiveBackground = _fallBackColors.TabInactiveBackground;
                TabInactiveUnderline = _fallBackColors.TabInactiveUnderline;
                TabInactiveForeground = _fallBackColors.TabInactiveForeground;

                Name = _fallBackName;
                Author = _fallBackAuthor;

                InEditMode = false;
            }
        }

        private bool NotPreInstalled()
        {
            return !_theme.PreInstalled;
        }

        private async Task Delete()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to delete this theme?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Edit()
        {
            _fallBackColors = new TerminalColors(_theme.Colors);
            _fallBackName = Name;
            _fallBackAuthor = Author;
            InEditMode = true;
        }

        private void SetActive()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        private Task Export()
        {
            var content = JsonConvert.SerializeObject(_theme, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new TerminalThemeContractResolver() });
            return _fileSystemService.SaveTextFile(Name, "Fluent Terminal Theme", ".flutecolors", content);
        }
    }
}