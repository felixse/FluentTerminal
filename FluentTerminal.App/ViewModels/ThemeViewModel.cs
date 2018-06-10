using FluentTerminal.App.Services;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;

namespace FluentTerminal.App.ViewModels
{
    public class ThemeViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private string _author;
        private Color _background;
        private double _backgroundOpacity;
        private Color _black;
        private Color _blue;
        private Color _brightBlack;
        private Color _brightBlue;
        private Color _brightCyan;
        private Color _brightGreen;
        private Color _brightMagenta;
        private Color _brightRed;
        private Color _brightWhite;
        private Color _brightYellow;
        private Color _cursor;
        private Color _cursorAccent;
        private Color _cyan;
        private string _fallBackAuthor;
        private TerminalColors _fallBackColors;
        private string _fallBackName;
        private Color _foreground;
        private Color _green;
        private bool _inEditMode;
        private bool _isActive;
        private Color _magenta;
        private string _name;
        private Color _red;
        private Color _selection;
        private readonly TerminalTheme _theme;
        private Color _white;
        private Color _yellow;

        public event EventHandler<Color> BackgroundChanged;

        public ThemeViewModel(TerminalTheme theme, ISettingsService settingsService, IDialogService dialogService)
        {
            _theme = theme;
            _settingsService = settingsService;
            _dialogService = dialogService;

            Name = _theme.Name;
            Author = _theme.Author;
            Id = _theme.Id;

            Black = _theme.Colors.Black.ToColor();
            Red = _theme.Colors.Red.ToColor();
            Green = _theme.Colors.Green.ToColor();
            Yellow = _theme.Colors.Yellow.ToColor();
            Blue = _theme.Colors.Blue.ToColor();
            Magenta = _theme.Colors.Magenta.ToColor();
            Cyan = _theme.Colors.Cyan.ToColor();
            White = _theme.Colors.White.ToColor();

            BrightBlack = _theme.Colors.BrightBlack.ToColor();
            BrightRed = _theme.Colors.BrightRed.ToColor();
            BrightGreen = _theme.Colors.BrightGreen.ToColor();
            BrightYellow = _theme.Colors.BrightYellow.ToColor();
            BrightBlue = _theme.Colors.BrightBlue.ToColor();
            BrightMagenta = _theme.Colors.BrightMagenta.ToColor();
            BrightCyan = _theme.Colors.BrightCyan.ToColor();
            BrightWhite = _theme.Colors.BrightWhite.ToColor();

            Background = _theme.Colors.Background.ToColor();
            Foreground = _theme.Colors.Foreground.ToColor();
            Cursor = _theme.Colors.Cursor.ToColor();
            CursorAccent = _theme.Colors.CursorAccent.ToColor();
            Selection = _theme.Colors.Selection.FromString();

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

        public Color Background
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

        public Color Black
        {
            get => _black;
            set => Set(ref _black, value);
        }

        public Color Blue
        {
            get => _blue;
            set => Set(ref _blue, value);
        }

        public Color BrightBlack
        {
            get => _brightBlack;
            set => Set(ref _brightBlack, value);
        }

        public Color BrightBlue
        {
            get => _brightBlue;
            set => Set(ref _brightBlue, value);
        }

        public Color BrightCyan
        {
            get => _brightCyan;
            set => Set(ref _brightCyan, value);
        }

        public Color BrightGreen
        {
            get => _brightGreen;
            set => Set(ref _brightGreen, value);
        }

        public Color BrightMagenta
        {
            get => _brightMagenta;
            set => Set(ref _brightMagenta, value);
        }

        public Color BrightRed
        {
            get => _brightRed;
            set => Set(ref _brightRed, value);
        }

        public Color BrightWhite
        {
            get => _brightWhite;
            set => Set(ref _brightWhite, value);
        }

        public Color BrightYellow
        {
            get => _brightYellow;
            set => Set(ref _brightYellow, value);
        }

        public RelayCommand CancelEditCommand { get; }

        public Color Cursor
        {
            get => _cursor;
            set => Set(ref _cursor, value);
        }

        public Color CursorAccent
        {
            get => _cursorAccent;
            set => Set(ref _cursorAccent, value);
        }

        public Color Cyan
        {
            get => _cyan;
            set => Set(ref _cyan, value);
        }

        public RelayCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand ExportCommand { get; }

        public Color Foreground
        {
            get => _foreground;
            set => Set(ref _foreground, value);
        }

        public Color Green
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

        public Color Magenta
        {
            get => _magenta;
            set => Set(ref _magenta, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public Color Red
        {
            get => _red;
            set => Set(ref _red, value);
        }

        public RelayCommand SaveChangesCommand { get; }

        public Color Selection
        {
            get => _selection;
            set => Set(ref _selection, value);
        }

        public RelayCommand SetActiveCommand { get; }

        public Color White
        {
            get => _white;
            set => Set(ref _white, value);
        }

        public Color Yellow
        {
            get => _yellow;
            set => Set(ref _yellow, value);
        }

        public void SaveChanges()
        {
            _theme.Name = Name;
            _theme.Author = Author;

            _theme.Colors.Black = Black.ToColorString(false);
            _theme.Colors.Red = Red.ToColorString(false);
            _theme.Colors.Green = Green.ToColorString(false);
            _theme.Colors.Yellow = Yellow.ToColorString(false);
            _theme.Colors.Blue = Blue.ToColorString(false);
            _theme.Colors.Magenta = Magenta.ToColorString(false);
            _theme.Colors.Cyan = Cyan.ToColorString(false);
            _theme.Colors.White = White.ToColorString(false);

            _theme.Colors.BrightBlack = BrightBlack.ToColorString(false);
            _theme.Colors.BrightRed = BrightRed.ToColorString(false);
            _theme.Colors.BrightGreen = BrightGreen.ToColorString(false);
            _theme.Colors.BrightYellow = BrightYellow.ToColorString(false);
            _theme.Colors.BrightBlue = BrightBlue.ToColorString(false);
            _theme.Colors.BrightMagenta = BrightMagenta.ToColorString(false);
            _theme.Colors.BrightCyan = BrightCyan.ToColorString(false);
            _theme.Colors.BrightWhite = BrightWhite.ToColorString(false);

            _theme.Colors.Background = Background.ToColorString(false);
            _theme.Colors.Foreground = Foreground.ToColorString(false);
            _theme.Colors.Cursor = Cursor.ToColorString(false);
            _theme.Colors.CursorAccent = CursorAccent.ToColorString(false);
            _theme.Colors.Selection = Selection.ToColorString(true);

            _settingsService.SaveTheme(_theme);

            InEditMode = false;
        }

        private async Task CancelEdit()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to discard all changes?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Black = _fallBackColors.Black.ToColor();
                Red = _fallBackColors.Red.ToColor();
                Green = _fallBackColors.Green.ToColor();
                Yellow = _fallBackColors.Yellow.ToColor();
                Blue = _fallBackColors.Blue.ToColor();
                Magenta = _fallBackColors.Magenta.ToColor();
                Cyan = _fallBackColors.Cyan.ToColor();
                White = _fallBackColors.White.ToColor();

                BrightBlack = _fallBackColors.BrightBlack.ToColor();
                BrightRed = _fallBackColors.BrightRed.ToColor();
                BrightGreen = _fallBackColors.BrightGreen.ToColor();
                BrightYellow = _fallBackColors.BrightYellow.ToColor();
                BrightBlue = _fallBackColors.BrightBlue.ToColor();
                BrightMagenta = _fallBackColors.BrightMagenta.ToColor();
                BrightCyan = _fallBackColors.BrightCyan.ToColor();
                BrightWhite = _fallBackColors.BrightWhite.ToColor();

                Background = _fallBackColors.Background.ToColor();
                Foreground = _fallBackColors.Foreground.ToColor();
                Cursor = _fallBackColors.Cursor.ToColor();
                CursorAccent = _fallBackColors.CursorAccent.ToColor();
                Selection = _fallBackColors.Selection.FromString();

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

        private async Task Export()
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.SuggestedFileName = Name;
            picker.FileTypeChoices.Add("Fluent Terminal Theme", new List<string> { ".flutecolors" });

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                var content = JsonConvert.SerializeObject(_theme, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new TerminalThemeContractResolver() });
                await FileIO.WriteTextAsync(file, content);
            }
        }
    }
}