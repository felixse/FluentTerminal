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
            DeleteCommand = new RelayCommand(async () => await Delete().ConfigureAwait(false), NotPreInstalled);
            EditCommand = new RelayCommand(Edit, NotPreInstalled);
            CancelEditCommand = new RelayCommand(async () => await CancelEdit().ConfigureAwait(false));
            SaveChangesCommand = new RelayCommand(async () => await SaveChanges().ConfigureAwait(false));
            ExportCommand = new RelayCommand(async () => await Export().ConfigureAwait(false), NotPreInstalled);
            ChooseBackgroundImageCommand = new RelayCommand(async () => await ChooseBackgroundImage(), NotPreInstalled);
            DeleteBackgroundImageCommand = new RelayCommand(async () => await DeleteBackgroundImageIfExists(), NotPreInstalled);
        }

        public event EventHandler Activated;

        public event EventHandler Deleted;

        public TerminalTheme Model { get; }

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

        public ImageFile BackgroundThemeFile
        {
            get => _backgroundThemeFile;
            set
            {
                Set(ref _backgroundThemeFile, value);
                BackgroundImageChanged?.Invoke(this, value);
            }
        }

        public RelayCommand ChooseBackgroundImageCommand { get; }

        public RelayCommand DeleteBackgroundImageCommand { get; }

        public async Task SaveChanges()
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
                await _imageFileSystemService.RemoveImportedImage(
                    $"{Model.BackgroundImage?.Name}{Model.BackgroundImage?.FileType}");
            }

            Model.BackgroundImage = await SaveBackgroundImage();

            _settingsService.SaveTheme(Model);

            InEditMode = false;
            _isNew = false;
        }

        private async Task CancelEdit()
        {
            if (_isNew)
            {
                await Delete();
            }
            else
            {
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
                    var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDiscardChanges"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

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

                        await _imageFileSystemService.RemoveTemporaryBackgroundThemeImage();
                    }
                }
                else
                {
                    InEditMode = false;
                }
            }
        }

        private bool NotPreInstalled()
        {
            return !Model.PreInstalled;
        }

        private async Task Delete()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDeleteTheme"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                await DeleteBackgroundImageIfExists();
                await _imageFileSystemService.RemoveTemporaryBackgroundThemeImage();

                Deleted?.Invoke(this, EventArgs.Empty);
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
            return _fileSystemService.SaveTextFile(Name, "Fluent Terminal Theme", ".flutecolors", content);
        }

        private async Task<ImageFile> SaveBackgroundImage()
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
                await _fileSystemService.SaveImageInRoaming(BackgroundThemeFile);

            await _imageFileSystemService.RemoveTemporaryBackgroundThemeImage();

            return importedBackgroundThemeFile;
        }

        private async Task ChooseBackgroundImage()
        {
            var choosenImage = await _imageFileSystemService.ImportTemporaryImageFile(new[] { ".jpeg", ".png", ".jpg" });

            if(choosenImage == null)
            {
                return;
            }

            BackgroundThemeFile = choosenImage;
        }

        private async Task DeleteBackgroundImageIfExists()
        {
            if (BackgroundThemeFile != null)
            {
                await _imageFileSystemService.RemoveImportedImage(
                    $"{Model.BackgroundImage?.Name}{Model.BackgroundImage?.FileType}");
            }
        }
    }
}