using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentTerminal.App.ViewModels
{
    public class ThemeViewModel : ObservableObject
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
        private TerminalTheme _fallbackTheme;
        private string _foreground;
        private string _green;
        private bool _inEditMode;
        private bool _isActive;
        private string _magenta;
        private string _name;
        private bool _isNew;
        private string _red;
        private string _selection;
        private string _white;
        private string _yellow;
        private ImageFile _backgroundThemeFile;
        private readonly IFileSystemService _fileSystemService;
        private readonly IImageFileSystemService _imageFileSystemService;

        public event EventHandler<string> BackgroundChanged;
        public event EventHandler<ImageFile> BackgroundImageChanged;

        public ThemeViewModel(TerminalTheme theme,
                              ISettingsService settingsService,
                              IDialogService dialogService,
                              IFileSystemService fileSystemService,
                              IImageFileSystemService imageFileSystemService,
                              bool isNew)
        {
            Model = theme;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _imageFileSystemService = imageFileSystemService;
            _isNew = isNew;

            Name = Model.Name;
            Author = Model.Author;
            Id = Model.Id;

            Black = Model.Colors.Black;
            Red = Model.Colors.Red;
            Green = Model.Colors.Green;
            Yellow = Model.Colors.Yellow;
            Blue = Model.Colors.Blue;
            Magenta = Model.Colors.Magenta;
            Cyan = Model.Colors.Cyan;
            White = Model.Colors.White;

            BrightBlack = Model.Colors.BrightBlack;
            BrightRed = Model.Colors.BrightRed;
            BrightGreen = Model.Colors.BrightGreen;
            BrightYellow = Model.Colors.BrightYellow;
            BrightBlue = Model.Colors.BrightBlue;
            BrightMagenta = Model.Colors.BrightMagenta;
            BrightCyan = Model.Colors.BrightCyan;
            BrightWhite = Model.Colors.BrightWhite;

            Background = Model.Colors.Background;
            Foreground = Model.Colors.Foreground;
            Cursor = Model.Colors.Cursor;
            CursorAccent = Model.Colors.CursorAccent;
            Selection = Model.Colors.Selection;

            BackgroundThemeFile = Model.BackgroundImage;

            SetActiveCommand = new RelayCommand(SetActive);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, NotPreInstalled);
            EditCommand = new RelayCommand(Edit, NotPreInstalled);
            CancelEditCommand = new AsyncRelayCommand(CancelEditAsync);
            SaveChangesCommand = new AsyncRelayCommand(SaveChangesAsync);
            ExportCommand = new AsyncRelayCommand(Export, NotPreInstalled);
            ChooseBackgroundImageCommand = new AsyncRelayCommand(ChooseBackgroundImageAsync, NotPreInstalled);
            DeleteBackgroundImageCommand = new AsyncRelayCommand(DeleteBackgroundImageAsync, NotPreInstalled);
        }

        public event EventHandler Activated;

        public event EventHandler Deleted;

        public TerminalTheme Model { get; }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Background
        {
            get => _background;
            set
            {
                SetProperty(ref _background, value);
                BackgroundChanged?.Invoke(this, value);
            }
        }

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => SetProperty(ref _backgroundOpacity, value);
        }

        public string Black
        {
            get => _black;
            set => SetProperty(ref _black, value);
        }

        public string Blue
        {
            get => _blue;
            set => SetProperty(ref _blue, value);
        }

        public string BrightBlack
        {
            get => _brightBlack;
            set => SetProperty(ref _brightBlack, value);
        }

        public string BrightBlue
        {
            get => _brightBlue;
            set => SetProperty(ref _brightBlue, value);
        }

        public string BrightCyan
        {
            get => _brightCyan;
            set => SetProperty(ref _brightCyan, value);
        }

        public string BrightGreen
        {
            get => _brightGreen;
            set => SetProperty(ref _brightGreen, value);
        }

        public string BrightMagenta
        {
            get => _brightMagenta;
            set => SetProperty(ref _brightMagenta, value);
        }

        public string BrightRed
        {
            get => _brightRed;
            set => SetProperty(ref _brightRed, value);
        }

        public string BrightWhite
        {
            get => _brightWhite;
            set => SetProperty(ref _brightWhite, value);
        }

        public string BrightYellow
        {
            get => _brightYellow;
            set => SetProperty(ref _brightYellow, value);
        }

        public ICommand CancelEditCommand { get; }

        public string Cursor
        {
            get => _cursor;
            set => SetProperty(ref _cursor, value);
        }

        public string CursorAccent
        {
            get => _cursorAccent;
            set => SetProperty(ref _cursorAccent, value);
        }

        public string Cyan
        {
            get => _cyan;
            set => SetProperty(ref _cyan, value);
        }

        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExportCommand { get; }

        public string Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        public string Green
        {
            get => _green;
            set => SetProperty(ref _green, value);
        }

        public Guid Id { get; }

        public bool InEditMode
        {
            get => _inEditMode;
            set => SetProperty(ref _inEditMode, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string Magenta
        {
            get => _magenta;
            set => SetProperty(ref _magenta, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Red
        {
            get => _red;
            set => SetProperty(ref _red, value);
        }

        public ICommand SaveChangesCommand { get; }

        public string Selection
        {
            get => _selection;
            set => SetProperty(ref _selection, value);
        }

        public ICommand SetActiveCommand { get; }

        public string White
        {
            get => _white;
            set => SetProperty(ref _white, value);
        }

        public string Yellow
        {
            get => _yellow;
            set => SetProperty(ref _yellow, value);
        }

        public ImageFile BackgroundThemeFile
        {
            get => _backgroundThemeFile;
            set
            {
                SetProperty(ref _backgroundThemeFile, value);
                BackgroundImageChanged?.Invoke(this, value);
            }
        }

        public ICommand ChooseBackgroundImageCommand { get; }

        public ICommand DeleteBackgroundImageCommand { get; }

        // Requires UI thread
        private async Task SaveChangesAsync()
        {
            Model.Name = Name;
            Model.Author = Author;

            Model.Colors.Black = Black;
            Model.Colors.Red = Red;
            Model.Colors.Green = Green;
            Model.Colors.Yellow = Yellow;
            Model.Colors.Blue = Blue;
            Model.Colors.Magenta = Magenta;
            Model.Colors.Cyan = Cyan;
            Model.Colors.White = White;

            Model.Colors.BrightBlack = BrightBlack;
            Model.Colors.BrightRed = BrightRed;
            Model.Colors.BrightGreen = BrightGreen;
            Model.Colors.BrightYellow = BrightYellow;
            Model.Colors.BrightBlue = BrightBlue;
            Model.Colors.BrightMagenta = BrightMagenta;
            Model.Colors.BrightCyan = BrightCyan;
            Model.Colors.BrightWhite = BrightWhite;

            Model.Colors.Background = Background;
            Model.Colors.Foreground = Foreground;
            Model.Colors.Cursor = Cursor;
            Model.Colors.CursorAccent = CursorAccent;
            Model.Colors.Selection = Selection;

            if (Model.BackgroundImage != null &&
               BackgroundThemeFile != Model?.BackgroundImage)
            {
                // ConfigureAwait(true) because we're setting some view-model properties afterwards.
                await _imageFileSystemService
                    .RemoveImportedImageAsync($"{Model.BackgroundImage?.Name}{Model.BackgroundImage?.FileType}")
                    .ConfigureAwait(true);
            }

            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            BackgroundThemeFile = await SaveBackgroundImageAsync().ConfigureAwait(true);

            Model.BackgroundImage = BackgroundThemeFile;

            _settingsService.SaveTheme(Model);

            InEditMode = false;
            _isNew = false;
        }

        // Requires UI thread
        private async Task CancelEditAsync()
        {
            if (_isNew)
            {
                await DeleteAsync().ConfigureAwait(false);

                return;
            }

            TerminalTheme changedTheme = new TerminalTheme()
            {
                Name = Name,
                Author = Author,
                BackgroundImage = BackgroundThemeFile,
                Colors = new TerminalColors()
                {
                    Black = Black,
                    Red = Red,
                    Green = Green,
                    Yellow = Yellow,
                    Blue = Blue,
                    Magenta = Magenta,
                    Cyan = Cyan,
                    White = White,

                    BrightBlack = BrightBlack,
                    BrightRed = BrightRed,
                    BrightGreen = BrightGreen,
                    BrightYellow = BrightYellow,
                    BrightBlue = BrightBlue,
                    BrightMagenta = BrightMagenta,
                    BrightCyan = BrightCyan,
                    BrightWhite = BrightWhite,

                    Background = Background,
                    Foreground = Foreground,
                    Cursor = Cursor,
                    CursorAccent = CursorAccent,
                    Selection = Selection
                }
            };

            if (!_fallbackTheme.Equals(changedTheme))
            {
                // ConfigureAwait(true) because we're setting some view-model properties afterwards.
                var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                    I18N.Translate("ConfirmDiscardChanges"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                if (result == DialogButton.OK)
                {
                    Black = _fallbackTheme.Colors.Black;
                    Red = _fallbackTheme.Colors.Red;
                    Green = _fallbackTheme.Colors.Green;
                    Yellow = _fallbackTheme.Colors.Yellow;
                    Blue = _fallbackTheme.Colors.Blue;
                    Magenta = _fallbackTheme.Colors.Magenta;
                    Cyan = _fallbackTheme.Colors.Cyan;
                    White = _fallbackTheme.Colors.White;

                    BrightBlack = _fallbackTheme.Colors.BrightBlack;
                    BrightRed = _fallbackTheme.Colors.BrightRed;
                    BrightGreen = _fallbackTheme.Colors.BrightGreen;
                    BrightYellow = _fallbackTheme.Colors.BrightYellow;
                    BrightBlue = _fallbackTheme.Colors.BrightBlue;
                    BrightMagenta = _fallbackTheme.Colors.BrightMagenta;
                    BrightCyan = _fallbackTheme.Colors.BrightCyan;
                    BrightWhite = _fallbackTheme.Colors.BrightWhite;

                    Background = _fallbackTheme.Colors.Background;
                    Foreground = _fallbackTheme.Colors.Foreground;
                    Cursor = _fallbackTheme.Colors.Cursor;
                    CursorAccent = _fallbackTheme.Colors.CursorAccent;
                    Selection = _fallbackTheme.Colors.Selection;

                    Name = _fallbackTheme.Name;
                    Author = _fallbackTheme.Author;

                    BackgroundThemeFile = _fallbackTheme.BackgroundImage;

                    InEditMode = false;

                    await _imageFileSystemService.RemoveTemporaryBackgroundThemeImageAsync().ConfigureAwait(false);
                }
            }
            else
            {
                InEditMode = false;
            }
        }

        private bool NotPreInstalled()
        {
            return !Model.PreInstalled;
        }

        // Requires UI thread
        private async Task DeleteAsync()
        {
            // ConfigureAwait(true) because we need to trigger Deleted event in the calling (UI) thread.
            var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmDeleteTheme"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                // ConfigureAwait(true) because we need to trigger Deleted event in the calling (UI) thread.
                await DeleteBackgroundImageIfExistsAsync().ConfigureAwait(true);
                // ConfigureAwait(true) because we need to trigger Deleted event in the calling (UI) thread.
                await _imageFileSystemService.RemoveTemporaryBackgroundThemeImageAsync().ConfigureAwait(true);

                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }

        // Requires UI thread
        private async Task DeleteBackgroundImageAsync()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                    I18N.Translate("ConfirmDeleteBackgroundImage"), DialogButton.OK, DialogButton.Cancel)
                .ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                BackgroundThemeFile = null;
            }
        }

        private void Edit()
        {
            _fallbackTheme = new TerminalTheme(Model);
            InEditMode = true;
        }

        private void SetActive()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        private Task Export()
        {
            var encodedImage = _imageFileSystemService.EncodeImage(BackgroundThemeFile);
            var exportedTheme = new ExportedTerminalTheme(Model, encodedImage);
            var content = JsonConvert.SerializeObject(exportedTheme, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new TerminalThemeContractResolver() });
            return _fileSystemService.SaveTextFileAsync(Name, "Fluent Terminal Theme", ".flutecolors", content);
        }

        private async Task<ImageFile> SaveBackgroundImageAsync()
        {
            if (BackgroundThemeFile == null)
            {
                return BackgroundThemeFile;
            }

            if (BackgroundThemeFile == Model.BackgroundImage)
            {
                return BackgroundThemeFile;
            }

            var importedBackgroundThemeFile =
                await _fileSystemService.SaveImageInRoamingAsync(BackgroundThemeFile).ConfigureAwait(false);

            await _imageFileSystemService.RemoveTemporaryBackgroundThemeImageAsync().ConfigureAwait(false);

            return importedBackgroundThemeFile;
        }

        // Requires UI thread
        private async Task ChooseBackgroundImageAsync()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var chosenImage = await _imageFileSystemService
                .ImportTemporaryImageFileAsync(new[] {".jpeg", ".png", ".jpg"}).ConfigureAwait(true);

            if(chosenImage != null)
            {
                BackgroundThemeFile = chosenImage;
            }
        }

        // Requires UI thread
        private async Task DeleteBackgroundImageIfExistsAsync()
        {
            if (BackgroundThemeFile != null)
            {
                var imageFile = Model.BackgroundImage;

                BackgroundThemeFile = null;

                if (imageFile != null)
                {
                    await _imageFileSystemService.RemoveImportedImageAsync($"{imageFile.Name}{imageFile.FileType}")
                        .ConfigureAwait(false);
                }
            }
        }
    }
}