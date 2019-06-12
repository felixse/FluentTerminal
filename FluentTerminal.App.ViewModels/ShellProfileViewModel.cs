using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
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
        #region Static

        private static readonly LineEndingStyle[] LineEndingStylesArray =
            Enum.GetValues(typeof(LineEndingStyle)).Cast<LineEndingStyle>().ToArray();

        #endregion Static

        #region Fields

        private ShellProfile _fallbackProfile;

        protected readonly IDialogService DialogService;
        protected readonly IFileSystemService FileSystemService;
        protected readonly ISettingsService SettingsService;
        protected readonly IApplicationView ApplicationView;
        protected readonly IDefaultValueProvider DefaultValueProvider;

        #endregion Fields

        #region Properties

        protected bool IsNew { get; set; }

        public Guid Id { get; private set; }

        public ShellProfile Model { get; private set; }

        public bool PreInstalled { get; private set; }

        private bool _useConPty;

        public bool UseConPty
        {
            get => _useConPty;
            set => Set(ref _useConPty, value);
        }

        private string _arguments;

        public string Arguments
        {
            get => _arguments;
            set => Set(ref _arguments, value);
        }

        private bool _inEditMode;

        public bool InEditMode
        {
            get => _inEditMode;
            set => Set(ref _inEditMode, value);
        }

        private bool _isDefault;

        public bool IsDefault
        {
            get => _isDefault;
            set => Set(ref _isDefault, value);
        }

        private string _location;

        public string Location
        {
            get => _location;
            set => Set(ref _location, value);
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private TabTheme _selectedTabTheme;

        public TabTheme SelectedTabTheme
        {
            get => _selectedTabTheme;
            set
            {
                if (value != null && Set(ref _selectedTabTheme, value))
                {
                    _tabThemeId = value.Id;
                }
            }
        }

        private int _tabThemeId;

        public int TabThemeId
        {
            get => _tabThemeId;
            set
            {
                if (Set(ref _tabThemeId, value))
                {
                    SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id.Equals(value));
                }
            }
        }

        private TerminalTheme _selectedTerminalTheme;

        public TerminalTheme SelectedTerminalTheme
        {
            get => _selectedTerminalTheme;
            set
            {
                if (value != null && Set(ref _selectedTerminalTheme, value))
                {
                    _terminalThemeId = value.Id;
                }
            }
        }

        private Guid _terminalThemeId;

        public Guid TerminalThemeId
        {
            get => _terminalThemeId;
            set
            {
                if (Set(ref _terminalThemeId, value))
                {
                    SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id.Equals(value));
                }
            }
        }

        private string _workingDirectory;

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => Set(ref _workingDirectory, value);
        }

        private LineEndingStyle _lineEndingTranslation;

        public LineEndingStyle LineEndingTranslation
        {
            get => _lineEndingTranslation;
            set => Set(ref _lineEndingTranslation, value);
        }

        public ObservableCollection<TabTheme> TabThemes { get; }

        public KeyBindingsViewModel KeyBindings { get; }

        public ObservableCollection<TerminalTheme> TerminalThemes { get; }

        public ObservableCollection<LineEndingStyle> LineEndingStyles { get; } =
            new ObservableCollection<LineEndingStyle>(LineEndingStylesArray);

        #endregion Properties

        #region Events

        public event EventHandler Deleted;

        public event EventHandler SetAsDefault;

        #endregion Events

        #region Commands

        public IAsyncCommand BrowseForCustomShellCommand { get; }

        public IAsyncCommand BrowseForWorkingDirectoryCommand { get; }

        public IAsyncCommand CancelEditCommand { get; }

        public IAsyncCommand DeleteCommand { get; }

        public RelayCommand EditCommand { get; }

        public IAsyncCommand SaveChangesCommand { get; }

        public RelayCommand SetDefaultCommand { get; }

        public IAsyncCommand RestoreDefaultsCommand { get; }

        public IAsyncCommand AddKeyboardShortcutCommand { get; }

        #endregion Commands

        #region Constrcutor

        public ShellProfileViewModel(ShellProfile shellProfile, ISettingsService settingsService,
            IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView,
            IDefaultValueProvider defaultValueProvider, bool isNew)
        {
            Model = shellProfile;
            SettingsService = settingsService;
            DialogService = dialogService;
            FileSystemService = fileSystemService;
            ApplicationView = applicationView;
            DefaultValueProvider = defaultValueProvider;
            IsNew = isNew;

            SettingsService.ThemeAdded += OnThemeAdded;
            SettingsService.ThemeDeleted += OnThemeDeleted;

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            TerminalThemes = new ObservableCollection<TerminalTheme>
            {
                new TerminalTheme
                {
                    Id = Guid.Empty,
                    Name = "Default"
                }
            };
            foreach (var theme in SettingsService.GetThemes())
            {
                TerminalThemes.Add(theme);
            }

            KeyBindings = new KeyBindingsViewModel(shellProfile.Id.ToString(), DialogService, string.Empty, false);

            InitializeViewModelPropertiesPrivate(shellProfile);

            SetDefaultCommand = new RelayCommand(SetDefault);
            DeleteCommand = new AsyncCommand(Delete, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEdit);
            SaveChangesCommand = new AsyncCommand(SaveChangesAsync);
            AddKeyboardShortcutCommand = new AsyncCommand(AddKeyboardShortcut);
            BrowseForCustomShellCommand = new AsyncCommand(BrowseForCustomShell);
            BrowseForWorkingDirectoryCommand = new AsyncCommand(BrowseForWorkingDirectory);
            RestoreDefaultsCommand = new AsyncCommand(RestoreDefaults);
        }

        #endregion Constrcutor

        #region Methods

        // Loads view model properties from the input shellProfile
        private void InitializeViewModelPropertiesPrivate(ShellProfile shellProfile)
        {
            Id = shellProfile.Id;
            Name = shellProfile.Name;
            Arguments = shellProfile.Arguments;
            Location = shellProfile.Location;
            WorkingDirectory = shellProfile.WorkingDirectory;
            TerminalThemeId = shellProfile.TerminalThemeId;
            TabThemeId = shellProfile.TabThemeId;
            PreInstalled = shellProfile.PreInstalled;
            LineEndingTranslation = shellProfile.LineEndingTranslation;
            UseConPty = shellProfile.UseConPty;

            KeyBindings.Clear();
            foreach (var keyBinding in shellProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList())
            {
                KeyBindings.Add(keyBinding);
            }
        }

        // Loads view model properties from the input shellProfile
        protected virtual void InitializeViewModelProperties(ShellProfile shellProfile) =>
            InitializeViewModelPropertiesPrivate(shellProfile);

        // Fills the input profile properties the view model properties. (Inverse of InitializeViewModelProperties()).
        protected virtual Task FillProfileAsync(ShellProfile profile)
        {
            profile.Id = Id;
            profile.Name = Name;
            profile.Arguments = Arguments;
            profile.Location = Location;
            profile.WorkingDirectory = WorkingDirectory;
            profile.TerminalThemeId = SelectedTerminalTheme.Id;
            profile.TabThemeId = SelectedTabTheme.Id;
            profile.PreInstalled = PreInstalled;
            profile.LineEndingTranslation = LineEndingTranslation;
            profile.UseConPty = UseConPty;
            profile.KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList();

            return Task.CompletedTask;
        }

        // Used to "remember" the current state.
        protected virtual async Task<ShellProfile> CreateProfileAsync()
        {
            var profile = new ShellProfile();

            await FillProfileAsync(profile);

            return profile;
        }

        private void OnThemeAdded(object sender, TerminalTheme e)
        {
            ApplicationView.RunOnDispatcherThread(() =>
            {
                TerminalThemes.Add(e);
            });
        }

        private void OnThemeDeleted(object sender, Guid e)
        {
            if (SelectedTerminalTheme.Id == e)
            {
                ApplicationView.RunOnDispatcherThread(() =>
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

        public virtual async Task SaveChangesAsync()
        {
            await FillProfileAsync(Model);

            SettingsService.SaveShellProfile(Model);

            KeyBindings.Editable = false;
            InEditMode = false;
            IsNew = false;
        }

        private async Task RestoreDefaults()
        {
            if (InEditMode || !PreInstalled)
            {
                throw new InvalidOperationException();
            }

            var result = await DialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmRestoreProfile"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Model = DefaultValueProvider.GetPreinstalledShellProfiles().FirstOrDefault(x => x.Id == Model.Id);
                InitializeViewModelProperties(Model);
                SettingsService.SaveShellProfile(Model);
            }
        }

        public Task AddKeyboardShortcut()
        {
            return KeyBindings.ShowAddKeyBindingDialog();
        }

        private async Task BrowseForCustomShell()
        {
            var file = await FileSystemService.OpenFile(new[] { ".exe" }).ConfigureAwait(true);
            if (file != null)
            {
                Location = file.Path;
            }
        }

        private async Task BrowseForWorkingDirectory()
        {
            var directory = await FileSystemService.BrowseForDirectory().ConfigureAwait(true);
            if (directory != null)
            {
                WorkingDirectory = directory;
            }
        }

        private async Task CancelEdit()
        {
            if (IsNew)
            {
                await Delete();
            }
            else
            {
                ShellProfile changedProfile = await CreateProfileAsync();

                if (!_fallbackProfile.EqualTo(changedProfile))
                {
                    var result = await DialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDiscardChanges"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                    if (result == DialogButton.OK)
                    {
                        // Cancelled, so rollback
                        InitializeViewModelProperties(_fallbackProfile);

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
            var result = await DialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDeleteProfile"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Edit()
        {
            _fallbackProfile = Model.Clone();

            KeyBindings.Editable = true;
            InEditMode = true;
        }

        private void SetDefault()
        {
            SetAsDefault?.Invoke(this, EventArgs.Empty);
        }

        #endregion Methods
    }
}