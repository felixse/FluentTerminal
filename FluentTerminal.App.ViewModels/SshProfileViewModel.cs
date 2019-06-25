using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class SshProfileViewModel : ShellProfileViewModel, ISshConnectionInfo
    {
        #region Fields

        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        // To prevent validating the existence of the same file multiple times because it's kinda expensive
        private string _validatedIdentityFile;

        #endregion Fields

        #region Properties

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

        #endregion Properties

        #region Commands

        public IAsyncCommand BrowseForIdentityFileCommand { get; }

        #endregion Commands

        #region Constructor

        public SshProfileViewModel(SshProfile sshProfile, ISettingsService settingsService,
            IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView,
            IDefaultValueProvider defaultValueProvider,
            ITrayProcessCommunicationService trayProcessCommunicationService, bool isNew) : base(
            sshProfile ?? new SshProfile(), settingsService, dialogService, fileSystemService, applicationView,
            defaultValueProvider, isNew)
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;

            InitializeViewModelPropertiesPrivate(sshProfile ?? new SshProfile());

            BrowseForIdentityFileCommand = new AsyncCommand(BrowseForIdentityFile);
        }

        #endregion Constructor

        #region Methods

        // Fills the remaining view model properties (those which aren't filled by the base method) from the input sshProfile
        private void InitializeViewModelPropertiesPrivate(SshProfile sshProfile)
        {
            Host = sshProfile.Host;
            SshPort = sshProfile.SshPort;

            Username = sshProfile.Username;

            if (string.IsNullOrEmpty(Username))
            {
                _trayProcessCommunicationService.GetUserName().ContinueWith(t =>
                {
                    if (string.IsNullOrEmpty(Username))
                    {
                        Username = t.Result;
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            IdentityFile = sshProfile.IdentityFile;
            UseMosh = sshProfile.UseMosh;
            MoshPortFrom = sshProfile.MoshPortFrom;
            MoshPortTo = sshProfile.MoshPortTo;
        }

        protected override void InitializeViewModelProperties(ShellProfile sshProfile)
        {
            base.InitializeViewModelProperties(sshProfile);

            InitializeViewModelPropertiesPrivate((SshProfile)sshProfile);
        }

        protected override async Task FillProfileAsync(ShellProfile profile)
        {
            await base.FillProfileAsync(profile);

            profile.Location = _useMosh ? ShellLocation.Mosh : ShellLocation.SSH;
            profile.Arguments = GetArgumentsString();
            profile.WorkingDirectory = null;

            var sshProfile = (SshProfile)profile;

            sshProfile.Host = Host;
            sshProfile.SshPort = SshPort;
            sshProfile.Username = Username;
            sshProfile.IdentityFile = IdentityFile;
            sshProfile.UseMosh = UseMosh;
            sshProfile.MoshPortFrom = MoshPortFrom;
            sshProfile.MoshPortTo = MoshPortTo;
        }

        public virtual Task AcceptChangesAsync() => FillProfileAsync(Model);

        protected override async Task<ShellProfile> CreateProfileAsync()
        {
            var profile = new SshProfile();

            await FillProfileAsync(profile);

            return profile;
        }

        private string GetArgumentsString()
        {
            StringBuilder sb = new StringBuilder();


            if (_sshPort != SshProfile.DefaultSshPort)
                sb.Append($"-p {_sshPort:#####} ");

            if (!string.IsNullOrEmpty(_identityFile))
                sb.Append($"-i \"{_identityFile}\" ");

            sb.Append($"{_username}@{_host}");

            if (_useMosh)
                sb.Append($" {_moshPortFrom}:{_moshPortTo}");

            return sb.ToString();
        }

        public override async Task SaveChangesAsync()
        {
            var result = await ValidateAsync();

            if (result != SshConnectionInfoValidationResult.Valid)
            {
                await DialogService.ShowMessageDialogAsnyc(I18N.Translate("InvalidInput"),
                    result.GetErrorString(Environment.NewLine), DialogButton.OK);

                return;
            }

            await AcceptChangesAsync();

            SettingsService.SaveSshProfile((SshProfile) Model, IsNew);

            KeyBindings.Editable = false;
            InEditMode = false;
            IsNew = false;
        }

        private async Task BrowseForIdentityFile()
        {
            var file = await FileSystemService.OpenFile(new[] { "*" }).ConfigureAwait(true);
            if (file != null)
            {
                IdentityFile = file.Path;
            }
        }

        public async Task<SshConnectionInfoValidationResult> ValidateAsync()
        {
            var result = this.GetValidationResult();

            var identityFile = _identityFile;

            if (!string.IsNullOrEmpty(identityFile) && !string.Equals(identityFile, _validatedIdentityFile,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (await _trayProcessCommunicationService.CheckFileExistsAsync(identityFile))
                {
                    _validatedIdentityFile = identityFile;
                }
                else
                {
                    result |= SshConnectionInfoValidationResult.IdentityFileDoesNotExist;
                }
            }

            return result;
        }

        public void SetValidatedIdentityFile(string identityFile)
        {
            _validatedIdentityFile = identityFile;
            IdentityFile = identityFile;
        }

        #endregion Methods
    }
}