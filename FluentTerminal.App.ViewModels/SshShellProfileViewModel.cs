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
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class SshShellProfileViewModel : ViewModelBase, ISshConnectionInfo
    {
        #region Constants

        private const string MoshExe = "mosh.exe";

        #endregion Constants

        #region Fields

        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystemService;
        private readonly ISettingsService _settingsService;
        private SshShellProfile _fallbackProfile;
        private readonly IApplicationView _applicationView;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private bool _isNew;

        #endregion Fields

        #region Properties

        private Guid _id;

        public Guid Id
        {
            get => _id;
            private set => Set(ref _id, value);
        }

        public SshShellProfile Model { get; private set; }

        public KeyBindingsViewModel KeyBindings { get; }

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
                    TabThemeId = value.Id;
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
                    SelectedTabTheme = TabThemes.First(t => t.Id.Equals(value));
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
                    TerminalThemeId = value.Id;
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
                    SelectedTerminalTheme = TerminalThemes.First(t => t.Id.Equals(value));
                }
            }
        }

        private string _host;

        public string Host
        {
            get => _host;
            set => Set(ref _host, value);
        }

        private ushort _sshPort;
        
        public ushort SshPort
        {
            get => _sshPort;
            set => Set(ref _sshPort, value);
        }

        private string _username;

        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
        }

        private string _identityFile;

        public string IdentityFile
        {
            get => _identityFile;
            set => Set(ref _identityFile, value);
        }

        private bool _useMosh;

        public bool UseMosh
        {
            get => _useMosh;
            set => Set(ref _useMosh, value);
        }

        private ushort _moshPortFrom;

        public ushort MoshPortFrom
        {
            get => _moshPortFrom;
            set => Set(ref _moshPortFrom, value);
        }

        private ushort _moshPortTo;

        public ushort MoshPortTo
        {
            get => _moshPortTo;
            set => Set(ref _moshPortTo, value);
        }

        private bool _useConPty;

        public bool UseConPty
        {
            get => _useConPty;
            set => Set(ref _useConPty, value);
        }

        private LineEndingStyle _lineEndingTranslation;

        public LineEndingStyle LineEndingTranslation
        {
            get => _lineEndingTranslation;
            set => Set(ref _lineEndingTranslation, value);
        }

        public ObservableCollection<LineEndingStyle> LineEndingStyles { get; } =
            new ObservableCollection<LineEndingStyle>(Enum.GetValues(typeof(LineEndingStyle)).Cast<LineEndingStyle>());

        public ObservableCollection<TabTheme> TabThemes { get; }

        public ObservableCollection<TerminalTheme> TerminalThemes { get; }

        #endregion Properties

        #region Commands

        public IAsyncCommand BrowseForIdentityFileCommand { get; }

        public IAsyncCommand CancelEditCommand { get; }

        public IAsyncCommand DeleteCommand { get; }

        public RelayCommand EditCommand { get; }

        public AsyncCommand SaveChangesCommand { get; }

        public RelayCommand SetDefaultCommand { get; }

        //public IAsyncCommand RestoreDefaultsCommand { get; }

        public IAsyncCommand AddKeyboardShortcutCommand { get; }

        public object CloneCommand
        {
            get { throw new NotImplementedException(); }
        }

        #endregion Commands

        #region Events

        public event EventHandler Deleted;
        public event EventHandler SetAsDefault;

        #endregion Events

        public SshShellProfileViewModel(SshShellProfile sshShellProfile, ISettingsService settingsService,
            IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, bool isNew)
        {
            Model = sshShellProfile ?? new SshShellProfile();
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _isNew = isNew;

            _settingsService.ThemeAdded += OnThemeAdded;
            _settingsService.ThemeDeleted += OnThemeDeleted;

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            TerminalThemes = new ObservableCollection<TerminalTheme>(_settingsService.GetThemes());
            TerminalThemes.Insert(0, new TerminalTheme {Id = Guid.Empty, Name = "Default"});

            KeyBindings = new KeyBindingsViewModel(sshShellProfile.Id.ToString(), _dialogService, string.Empty, false);

            InitializeViewModelProperties(sshShellProfile);

            SetDefaultCommand = new RelayCommand(SetDefault);
            DeleteCommand = new AsyncCommand(Delete, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEdit);
            SaveChangesCommand = new AsyncCommand(SaveChanges);
            AddKeyboardShortcutCommand = new AsyncCommand(AddKeyboardShortcut);
            //RestoreDefaultsCommand = new AsyncCommand(RestoreDefaults);
            BrowseForIdentityFileCommand = new AsyncCommand(BrowseForIdentityFile);
        }

        #region Methods

        private string GetArgumentsString()
        {
            StringBuilder sb = new StringBuilder();


            if (_sshPort != SshShellProfile.DefaultSshPort)
                sb.Append($"-p {_sshPort:#####} ");

            if (!string.IsNullOrEmpty(_identityFile))
                sb.Append($"-i \"{_identityFile}\" ");

            sb.Append($"{_username}@{_host}");

            if (_useMosh)
                sb.Append($" {_moshPortFrom}:{_moshPortTo}");

            return sb.ToString();
        }

        private void InitializeViewModelProperties(SshShellProfile sshShellProfile)
        {
            SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id == sshShellProfile.TerminalThemeId);
            Id = sshShellProfile.Id;
            Name = sshShellProfile.Name;

            Host = sshShellProfile.Host;
            SshPort = sshShellProfile.SshPort;
            Username = sshShellProfile.Username;
            IdentityFile = sshShellProfile.IdentityFile;
            UseMosh = sshShellProfile.UseMosh;
            MoshPortFrom = sshShellProfile.MoshPortFrom;
            MoshPortTo = sshShellProfile.MoshPortTo;

            UseConPty = sshShellProfile.UseConPty;

            SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == sshShellProfile.TabThemeId);
            LineEndingTranslation = sshShellProfile.LineEndingTranslation;

            KeyBindings.Clear();
            foreach (var keyBinding in sshShellProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList())
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

        public async Task SaveChanges()
        {
            var result = Validate();

            if (result != SshConnectionInfoValidationResult.Valid)
            {
                await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("InvalidInput"),
                    result.GetErrorString(Environment.NewLine), DialogButton.OK);

                return;
            }

            await AcceptChangesAsync();

            _settingsService.SaveSshShellProfile(Model, _isNew);

            KeyBindings.Editable = false;
            InEditMode = false;
            _isNew = false;
        }

        public async Task AcceptChangesAsync()
        {
            Model.Location = await _trayProcessCommunicationService.GetMoshSshPath(_useMosh);
            Model.Arguments = GetArgumentsString();
            Model.Host = Host;
            Model.SshPort = SshPort;
            Model.Username = Username;
            Model.IdentityFile = IdentityFile;
            Model.UseMosh = UseMosh;
            Model.MoshPortFrom = MoshPortFrom;
            Model.MoshPortTo = MoshPortTo;

            Model.UseConPty = UseConPty;

            Model.Name = Name;
            Model.TabThemeId = SelectedTabTheme.Id;
            Model.TerminalThemeId = SelectedTerminalTheme.Id;
            Model.LineEndingTranslation = LineEndingTranslation;
            Model.KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList();
        }

        public Task AddKeyboardShortcut()
        {
            return KeyBindings.ShowAddKeyBindingDialog();
        }

        private async Task BrowseForIdentityFile()
        {
            var file = await _fileSystemService.OpenFile(new[] { "*" }).ConfigureAwait(true);
            if (file != null)
            {
                IdentityFile = file.Path;
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
                SshShellProfile changedProfile = new SshShellProfile
                {
                    Id = Id,
                    Name = Name,
                    TabThemeId = SelectedTabTheme.Id,
                    TerminalThemeId = SelectedTerminalTheme.Id,
                    LineEndingTranslation = LineEndingTranslation,

                    Host = Host,
                    SshPort = SshPort,
                    Username = Username,
                    IdentityFile = IdentityFile,
                    UseMosh = UseMosh,
                    MoshPortFrom = MoshPortFrom,
                    MoshPortTo = MoshPortTo,

                    UseConPty = UseConPty,

                    KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList()
                };

                if (!_fallbackProfile.EqualTo(changedProfile))
                {
                    var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDiscardChanges"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

                    if (result == DialogButton.OK)
                    {
                        Name = _fallbackProfile.Name;

                        Host = _fallbackProfile.Host;
                        SshPort = _fallbackProfile.SshPort;
                        Username = _fallbackProfile.Username;
                        IdentityFile = _fallbackProfile.IdentityFile;
                        UseMosh = _fallbackProfile.UseMosh;
                        MoshPortFrom = _fallbackProfile.MoshPortFrom;
                        MoshPortTo = _fallbackProfile.MoshPortTo;

                        UseConPty = _fallbackProfile.UseConPty;

                        LineEndingTranslation = _fallbackProfile.LineEndingTranslation;
                        SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id == _fallbackProfile.TerminalThemeId);
                        SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == _fallbackProfile.TabThemeId);

                        KeyBindings.KeyBindings.Clear();
                        foreach (var keyBinding in _fallbackProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList())
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
            var result = await _dialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"), I18N.Translate("ConfirmDeleteTheme"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Edit()
        {
            _fallbackProfile = new SshShellProfile
            {
                Id = Id,
                Name = Name,
                Host = Host,
                SshPort = SshPort,
                Username = Username,
                IdentityFile = IdentityFile,
                UseMosh = UseMosh,
                MoshPortFrom = MoshPortFrom,
                MoshPortTo = MoshPortTo,
                UseConPty = UseConPty,
                LineEndingTranslation = LineEndingTranslation,
                // ReSharper disable PossibleNullReferenceException
                TerminalThemeId = TerminalThemes.FirstOrDefault(t => t.Id == SelectedTerminalTheme.Id).Id,
                TabThemeId = TabThemes.FirstOrDefault(t => t.Id == SelectedTabTheme.Id).Id
                // ReSharper restore PossibleNullReferenceException
            };

            foreach (var keyBinding in Model.KeyBindings.Select(x => new KeyBinding(x)).ToList())
            {
                _fallbackProfile.KeyBindings.Add(keyBinding);
            }

            KeyBindings.Editable = true;
            InEditMode = true;
        }

        private void SetDefault()
        {
            SetAsDefault?.Invoke(this, EventArgs.Empty);
        }

        public SshConnectionInfoValidationResult Validate(bool allowNoUser = false) =>
            this.GetValidationResult(allowNoUser);

        #endregion Methods
    }
}