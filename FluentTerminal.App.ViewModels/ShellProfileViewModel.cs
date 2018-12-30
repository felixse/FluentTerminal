using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
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
        private ShellProfile _shellProfile;
        private string _arguments;
        private ShellProfile _fallbackProfile;
        private bool _inEditMode;
        private bool _isDefault;
        private string _location;
        private string _name;
        private TabTheme _selectedTabTheme;
        private TerminalTheme _selectedTerminalTheme;
        private string _workingDirectory;
        private readonly IApplicationView _applicationView;

        public ShellProfileViewModel(ShellProfile shellProfile, ISettingsService settingsService, IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView)
        {
            _shellProfile = shellProfile;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            TerminalThemes = new ObservableCollection<TerminalTheme>();
            TerminalThemes.Add(new TerminalTheme
            {
                Id = Guid.Empty,
                Name = "Default"
            });
            foreach(var theme in _settingsService.GetThemes())
            {
                TerminalThemes.Add(theme);
            }
            SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id == shellProfile.TerminalThemeId);

            Id = shellProfile.Id;
            Name = shellProfile.Name;
            Arguments = shellProfile.Arguments;
            Location = shellProfile.Location;
            WorkingDirectory = shellProfile.WorkingDirectory;
            SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == shellProfile.TabThemeId);
            PreInstalled = shellProfile.PreInstalled;

            _settingsService.ThemeAdded += OnThemeAdded;
            _settingsService.ThemeDeleted += OnThemeDeleted;

            var keyBindings = shellProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList();
            KeyBindings = new KeyBindingsViewModel(shellProfile.Id.ToString(), keyBindings, _dialogService, string.Empty, false);

            SetDefaultCommand = new RelayCommand(SetDefault);
            DeleteCommand = new AsyncCommand(Delete, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEdit);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            AddKeyboardShortcutCommand = new AsyncCommand(AddKeyboardShortcut);
            BrowseForCustomShellCommand = new AsyncCommand(BrowseForCustomShell);
            BrowseForWorkingDirectoryCommand = new AsyncCommand(BrowseForWorkingDirectory);
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
                    _shellProfile.TerminalThemeId = Guid.Empty;
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
        public IAsyncCommand AddKeyboardShortcutCommand { get; }
        public ObservableCollection<TabTheme> TabThemes { get; }
        public KeyBindingsViewModel KeyBindings { get; }

        public ObservableCollection<TerminalTheme> TerminalThemes { get; }

        public bool PreInstalled { get; }

        public string Arguments
        {
            get => _arguments;
            set => Set(ref _arguments, value);
        }

        public Guid Id { get; }

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

        public void SaveChanges()
        {
            _shellProfile.Arguments = Arguments;
            _shellProfile.Location = Location;
            _shellProfile.Name = Name;
            _shellProfile.WorkingDirectory = WorkingDirectory;
            _shellProfile.TabThemeId = SelectedTabTheme.Id;
            _shellProfile.TerminalThemeId = SelectedTerminalTheme.Id;
            _shellProfile.KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList();
            _settingsService.SaveShellProfile(_shellProfile);

            KeyBindings.Editable = false;
            InEditMode = false;
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
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to discard all changes?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Arguments = _fallbackProfile.Arguments;
                Location = _fallbackProfile.Location;
                Name = _fallbackProfile.Name;
                WorkingDirectory = _fallbackProfile.WorkingDirectory;
                SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id == _fallbackProfile.TerminalThemeId);
                SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == _fallbackProfile.TabThemeId);

                KeyBindings.KeyBindings.Clear();
                foreach (var keyBinding in _shellProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList())
                {
                    KeyBindings.Add(keyBinding);
                }

                KeyBindings.Editable = false;
                InEditMode = false;
            }
        }

        private bool CanDelete()
        {
            return !_shellProfile.PreInstalled;
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
                Arguments = _shellProfile.Arguments,
                Location = _shellProfile.Location,
                Name = _shellProfile.Name,
                WorkingDirectory = _shellProfile.WorkingDirectory,
                TabThemeId = _shellProfile.TabThemeId,
                TerminalThemeId = _shellProfile.TerminalThemeId
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