using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class ShellProfileViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystemService;
        private readonly ISettingsService _settingsService;
        private string _arguments;
        private ShellProfile _fallbackProfile;
        private bool _inEditMode;
        private bool _isDefault;
        private string _location;
        private LineEndingStyle _lineEndingStyle;
        private string _name;
        private bool _isNew;
        private TabTheme _selectedTabTheme;
        private TerminalTheme _selectedTerminalTheme;
        private string _workingDirectory;
        private readonly IApplicationView _applicationView;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private bool _editingLineEndingStyle;

        public ShellProfileViewModel(ShellProfile shellProfile, ISettingsService settingsService, IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView, IDefaultValueProvider defaultValueProvider, Boolean isNew)
        {
            Model = shellProfile;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;
            _defaultValueProvider = defaultValueProvider;
            _isNew = isNew;

            _settingsService.ThemeAdded += OnThemeAdded;
            _settingsService.ThemeDeleted += OnThemeDeleted;

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            TerminalThemes = new ObservableCollection<TerminalTheme>();
            TerminalThemes.Add(new TerminalTheme
            {
                Id = Guid.Empty,
                Name = "Default"
            });
            foreach (var theme in _settingsService.GetThemes())
            {
                TerminalThemes.Add(theme);
            }

            KeyBindings = new KeyBindingsViewModel(shellProfile.Id.ToString(), _dialogService, string.Empty, false);

            InitializeViewModelProperties(shellProfile);

            SetDefaultCommand = new RelayCommand(SetDefault);
            DeleteCommand = new AsyncCommand(Delete, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEdit);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            AddKeyboardShortcutCommand = new AsyncCommand(AddKeyboardShortcut);
            BrowseForCustomShellCommand = new AsyncCommand(BrowseForCustomShell);
            BrowseForWorkingDirectoryCommand = new AsyncCommand(BrowseForWorkingDirectory);
            RestoreDefaultsCommand = new AsyncCommand(RestoreDefaults);
        }

        private void InitializeViewModelProperties(ShellProfile shellProfile)
        {
            SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id == shellProfile.TerminalThemeId);
            Id = shellProfile.Id;
            Name = shellProfile.Name;
            Arguments = shellProfile.Arguments;
            Location = shellProfile.Location;
            WorkingDirectory = shellProfile.WorkingDirectory;
            SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == shellProfile.TabThemeId);
            PreInstalled = shellProfile.PreInstalled;
            LineEndingStyle = shellProfile.LineEndingTranslation;

            KeyBindings.Clear();
            foreach (var keyBinding in shellProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList())
            {
                KeyBindings.Add(keyBinding);
            }
        }

        private void OnThemeAdded(object sender, TerminalTheme e)
        {
            _applicationView.RunOnDispatcherThread(() =>
            {
                TerminalThemes.Add(e);
            });
        }

        private void OnThemeDeleted(object sender, Guid e)
        {
            if (SelectedTerminalTheme.Id == e)
            {
                _applicationView.RunOnDispatcherThread(() =>
                {
                    SelectedTerminalTheme = TerminalThemes.FirstOrDefault(x => x.Id == Guid.Empty);
                    Model.TerminalThemeId = Guid.Empty;
                    if (_fallbackProfile != null)
                    {
                        _fallbackProfile.TerminalThemeId = Guid.Empty;
                    }
                });
            }
        }

        public event EventHandler Deleted;
        public event EventHandler SetAsDefault;

        public IAsyncCommand BrowseForCustomShellCommand { get; }
        public IAsyncCommand BrowseForWorkingDirectoryCommand { get; }
        public IAsyncCommand CancelEditCommand { get; }
        public IAsyncCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public IAsyncCommand RestoreDefaultsCommand { get; }
        public IAsyncCommand AddKeyboardShortcutCommand { get; }
        public ObservableCollection<TabTheme> TabThemes { get; }
        public KeyBindingsViewModel KeyBindings { get; }

        public ObservableCollection<TerminalTheme> TerminalThemes { get; }
        public ShellProfile Model { get; private set; }
        public bool PreInstalled { get; private set; }

        public string Arguments
        {
            get => _arguments;
            set => Set(ref _arguments, value);
        }

        public Guid Id { get; private set; }

        public bool InEditMode
        {
            get => _inEditMode;
            set => Set(ref _inEditMode, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => Set(ref _isDefault, value);
        }

        public string Location
        {
            get => _location;
            set => Set(ref _location, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public TabTheme SelectedTabTheme
        {
            get => _selectedTabTheme;
            set
            {
                if (value != null)
                {
                    Set(ref _selectedTabTheme, value);
                }
            }
        }

        public TerminalTheme SelectedTerminalTheme
        {
            get => _selectedTerminalTheme;
            set
            {
                if (value != null)
                {
                    Set(ref _selectedTerminalTheme, value);
                }
            }
        }

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => Set(ref _workingDirectory, value);
        }

        public LineEndingStyle LineEndingStyle
        {
            get => _lineEndingStyle;
            set
            {
                if (_lineEndingStyle != value && !_editingLineEndingStyle)
                {
                    _editingLineEndingStyle = true;
                    _lineEndingStyle = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DoNotModifyIsSelected));
                    RaisePropertyChanged(nameof(ToCRIsSelected));
                    RaisePropertyChanged(nameof(ToCRLFIsSelected));
                    RaisePropertyChanged(nameof(ToLFIsSelected));
                    _editingLineEndingStyle = false;
                }
            }
        }

        public bool DoNotModifyIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.DoNotModify;
            set => LineEndingStyle = LineEndingStyle.DoNotModify;
        }

        public bool ToCRLFIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.ToCRLF;
            set => LineEndingStyle = LineEndingStyle.ToCRLF;
        }

        public bool ToLFIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.ToLF;
            set => LineEndingStyle = LineEndingStyle.ToLF;
        }

        public bool ToCRIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.ToCR;
            set => LineEndingStyle = LineEndingStyle.ToCR;
        }

        public void SaveChanges()
        {
            Model.Arguments = Arguments;
            Model.Location = Location;
            Model.Name = Name;
            Model.WorkingDirectory = WorkingDirectory;
            Model.TabThemeId = SelectedTabTheme.Id;
            Model.TerminalThemeId = SelectedTerminalTheme.Id;
            Model.LineEndingTranslation = _lineEndingStyle;
            Model.KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList();
            _settingsService.SaveShellProfile(Model);

            KeyBindings.Editable = false;
            InEditMode = false;
            _isNew = false;
        }

        private async Task RestoreDefaults()
        {
            if (InEditMode || !PreInstalled)
            {
                throw new InvalidOperationException();
            }

            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to restore this profile?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Model = _defaultValueProvider.GetPreinstalledShellProfiles().FirstOrDefault(x => x.Id == Model.Id);
                InitializeViewModelProperties(Model);
                _settingsService.SaveShellProfile(Model);
            }
        }

        public Task AddKeyboardShortcut()
        {
            return KeyBindings.ShowAddKeyBindingDialog();
        }

        private async Task BrowseForCustomShell()
        {
            var file = await _fileSystemService.OpenFile(new[] { ".exe" }).ConfigureAwait(true);
            if (file != null)
            {
                Location = file.Path;
            }
        }

        private async Task BrowseForWorkingDirectory()
        {
            var directory = await _fileSystemService.BrowseForDirectory().ConfigureAwait(true);
            if (directory != null)
            {
                WorkingDirectory = directory;
            }
        }

        private async Task CancelEdit()
        {
            if (_isNew)
            {
                await Delete();
            }
            else
            {
                ShellProfile changedProfile = new ShellProfile
                {
                    Arguments = Arguments,
                    Location = Location,
                    Name = Name,
                    WorkingDirectory = WorkingDirectory,
                    TabThemeId = SelectedTabTheme.Id,
                    TerminalThemeId = SelectedTerminalTheme.Id,
                    LineEndingTranslation = _lineEndingStyle,
                    KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList()
                };

                if (!_fallbackProfile.Equals(changedProfile))
                {
                    var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to discard all changes?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                    if (result == DialogButton.OK)
                    {
                        Arguments = _fallbackProfile.Arguments;
                        Location = _fallbackProfile.Location;
                        Name = _fallbackProfile.Name;
                        WorkingDirectory = _fallbackProfile.WorkingDirectory;
                        LineEndingStyle = _fallbackProfile.LineEndingTranslation;
                        SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id == _fallbackProfile.TerminalThemeId);
                        SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == _fallbackProfile.TabThemeId);

                        KeyBindings.KeyBindings.Clear();
                        foreach (var keyBinding in Model.KeyBindings.Select(x => new KeyBinding(x)).ToList())
                        {
                            KeyBindings.Add(keyBinding);
                        }

                        KeyBindings.Editable = false;
                        InEditMode = false;
                    }
                }
                else
                {
                    KeyBindings.Editable = false;
                    InEditMode = false;
                }
            }
        }

        private bool CanDelete()
        {
            return !Model.PreInstalled;
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
            // todo write copy ctor
            _fallbackProfile = new ShellProfile
            {
                Arguments = Model.Arguments,
                Location = Model.Location,
                Name = Model.Name,
                WorkingDirectory = Model.WorkingDirectory,
                TabThemeId = Model.TabThemeId,
                TerminalThemeId = Model.TerminalThemeId
            };

            KeyBindings.Editable = true;
            InEditMode = true;
        }

        private void SetDefault()
        {
            SetAsDefault?.Invoke(this, EventArgs.Empty);
        }
    }
}