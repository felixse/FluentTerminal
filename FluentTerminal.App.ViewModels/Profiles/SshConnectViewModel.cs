using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels.Profiles
{
    /// <summary>
    /// View-model for rich-UI based SSH profiles.
    /// </summary>
    public class SshConnectViewModel : ProfileProviderViewModelBase
    {
        #region Static

        public static string GetErrorString(SshConnectionInfoValidationResult result, string separator = "; ") =>
            string.Join(separator, GetErrors(result));

        public static IEnumerable<string> GetErrors(SshConnectionInfoValidationResult result)
        {
            if (result == SshConnectionInfoValidationResult.Valid)
            {
                yield break;
            }

            foreach (var value in Enum.GetValues(typeof(SshConnectionInfoValidationResult))
                .Cast<SshConnectionInfoValidationResult>().Where(r => r != SshConnectionInfoValidationResult.Valid))
            {
                if ((value & result) == value)
                {
                    yield return I18N.Translate($"{nameof(SshConnectionInfoValidationResult)}.{value}");
                }
            }
        }

        #endregion Static

        #region Fields

        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IFileSystemService _fileSystemService;

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

        public SshConnectViewModel(ISettingsService settingsService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, IFileSystemService fileSystemService,
            SshProfile original = null) : base(settingsService, applicationView, true,
            original ?? new SshProfile { UseMosh = settingsService.GetApplicationSettings().UseMoshByDefault,
                RequestConPty = settingsService.GetApplicationSettings().UseConPty
            })
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _fileSystemService = fileSystemService;

            Initialize((SshProfile)Model);

            BrowseForIdentityFileCommand = new AsyncCommand(BrowseForIdentityFile);
        }

        #endregion Constructor

        #region Methods

        // Fills the view model properties from the input sshProfile
        private void Initialize(SshProfile sshProfile)
        {
            Host = sshProfile.Host;
            SshPort = sshProfile.SshPort;

            Username = sshProfile.Username;

            if (string.IsNullOrEmpty(Username))
            {
                _trayProcessCommunicationService.GetUserNameAsync().ContinueWith(t =>
                {
                    var username = t.Result;

                    if (string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(username))
                    {
                        ApplicationView.ExecuteOnUiThreadAsync(() => Username = username);
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            IdentityFile = sshProfile.IdentityFile;
            UseMosh = sshProfile.UseMosh;
            MoshPortFrom = sshProfile.MoshPortFrom;
            MoshPortTo = sshProfile.MoshPortTo;
        }

        private async Task BrowseForIdentityFile()
        {
            var file = await _fileSystemService.OpenFile(new[] { "*" });
            if (file != null)
            {
                IdentityFile = file.Path;
            }
        }

        private string GetArgumentsString()
        {
            var sshArguments = GetSshArguments();

            if (!_useMosh)
            {
                return string.IsNullOrWhiteSpace(sshArguments) ? $"{_username}@{_host}" : $"{sshArguments} {_username}@{_host}";
            }

            return string.IsNullOrEmpty(sshArguments)
                ? $"-p {_moshPortFrom}:{_moshPortTo} {_username}@{_host}"
                : $"-p {_moshPortFrom}:{_moshPortTo} --ssh=\"ssh {sshArguments}\" {_username}@{_host}";
        }

        private string GetSshArguments()
        {
            if (string.IsNullOrEmpty(_identityFile))
            {
                return _sshPort == SshProfile.DefaultSshPort ? null : $"-p {_sshPort:#####}";
            }

            return _sshPort == SshProfile.DefaultSshPort
                ? $"-i '{_identityFile}'"
                : $"-p {_sshPort:#####} -i '{_identityFile}'";
        }

        protected override void LoadFromProfile(ShellProfile profile)
        {
            base.LoadFromProfile(profile);

            Initialize((SshProfile)profile);
        }

        protected override async Task CopyToProfileAsync(ShellProfile profile)
        {
            await base.CopyToProfileAsync(profile);

            SshProfile sshProfile = (SshProfile) profile;

            sshProfile.Location = _useMosh ? Constants.MoshCommandName : Constants.SshCommandName;
            sshProfile.Arguments = GetArgumentsString();

            sshProfile.WorkingDirectory = null;

            sshProfile.Host = _host;
            sshProfile.SshPort = _sshPort;
            sshProfile.Username = _username;
            sshProfile.IdentityFile = _identityFile;
            sshProfile.UseMosh = _useMosh;
            sshProfile.MoshPortFrom = _moshPortFrom;
            sshProfile.MoshPortTo = _moshPortTo;
        }

        public override async Task<string> ValidateAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            var result = await GetSshInfoValidationResultAsync();

            if (result == SshConnectionInfoValidationResult.Valid)
            {
                return null;
            }

            error = GetErrorString(result, Environment.NewLine);

            if (string.IsNullOrEmpty(error))
            {
                error = "Invalid input.";
            }

            return error;
        }

        public override bool HasChanges()
        {
            var original = (SshProfile)Model;

            return base.HasChanges() || !original.Host.NullableEqualTo(_host) || original.SshPort != _sshPort ||
                   !original.Username.NullableEqualTo(_username) ||
                   !original.IdentityFile.NullableEqualTo(_identityFile) || original.UseMosh != _useMosh ||
                   original.MoshPortFrom != _moshPortFrom || original.MoshPortTo != _moshPortTo;
        }

        public async Task<SshConnectionInfoValidationResult> GetSshInfoValidationResultAsync()
        {
            var result = SshConnectionInfoValidationResult.Valid;

            if (string.IsNullOrEmpty(_username))
            {
                result |= SshConnectionInfoValidationResult.UsernameEmpty;
            }

            if (string.IsNullOrEmpty(_host))
            {
                result |= SshConnectionInfoValidationResult.HostEmpty;
            }

            if (_sshPort < 1)
            {
                result |= SshConnectionInfoValidationResult.SshPortZeroOrNegative;
            }

            if (!await CheckIdentityFileExistsAsync())
            {
                result |= SshConnectionInfoValidationResult.IdentityFileDoesNotExist;
            }

            if (!_useMosh)
            {
                return result;
            }

            if (_moshPortFrom < 1)
            {
                result |= SshConnectionInfoValidationResult.MoshPortZeroOrNegative;
            }

            if (_moshPortFrom > _moshPortTo)
            {
                result |= SshConnectionInfoValidationResult.MoshPortRangeInvalid;
            }

            return result;
        }

        private async Task<bool> CheckIdentityFileExistsAsync()
        {
            var identityFile = _identityFile;

            if (string.IsNullOrEmpty(identityFile) ||
                identityFile.Equals(_validatedIdentityFile, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Here we need to take into account that files from ssh config dir can be provided by name, without full path.
            string fullPath;

            if (Path.IsPathRooted(identityFile))
            {
                fullPath = identityFile;
            }
            else
            {
                var sshConfigDir = await _trayProcessCommunicationService.GetSshConfigDirAsync();

                if (string.IsNullOrEmpty(sshConfigDir))
                {
                    return false;
                }

                fullPath = Path.Combine(sshConfigDir, identityFile);
            }

            if (await _trayProcessCommunicationService.CheckFileExistsAsync(fullPath))
            {
                _validatedIdentityFile = identityFile;

                IdentityFile = fullPath;

                return true;
            }

            return false;
        }

        public void SetValidatedIdentityFile(string identityFile)
        {
            _validatedIdentityFile = identityFile;
            IdentityFile = identityFile;
        }

        #endregion Methods

        #region Links/shortcuts related

        // Resources:
        // https://tools.ietf.org/html/draft-ietf-secsh-scp-sftp-ssh-uri-04#section-3
        // https://man.openbsd.org/ssh

        private const string SshUriScheme = "ssh";
        private const string MoshUriScheme = "mosh";

        // Constant derived from https://man.openbsd.org/ssh
        private const string IdentityFileOptionName = "IdentityFile";

        private static readonly string[] ValidMoshPortsNames = { "mosh_ports", "mosh-ports" };

        private static readonly Regex MoshRangeRx =
            new Regex(@"^(?<from>\d{1,5})[:-](?<to>\d{1,5})$", RegexOptions.Compiled);

        public static bool CheckScheme(Uri uri) =>
            SshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase) ||
            MoshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase);

        public static SshConnectViewModel ParseUri(Uri uri, ISettingsService settingsService,
            IApplicationView applicationView, ITrayProcessCommunicationService trayProcessCommunicationService,
            IFileSystemService fileSystemService, IApplicationDataContainer historyContainer)
        {
            var vm = new SshConnectViewModel(settingsService, applicationView, trayProcessCommunicationService,
                fileSystemService)
            {
                Host = uri.Host,
                UseMosh = MoshUriScheme.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase)
            };

            if (uri.Port >= 0)
                vm.SshPort = (ushort)uri.Port;

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                string[] parts = uri.UserInfo.Split(';');

                if (parts.Length > 2)
                    throw new FormatException($"UserInfo part contains {parts.Length} elements.");

                vm.Username = HttpUtility.UrlDecode(parts[0]);

                if (parts.Length > 1)
                {
                    // For now we are only interested in IdentityFile option
                    Tuple<string, string> identityFileOption = ParseParams(parts[1], ',').FirstOrDefault(p =>
                        string.Equals(p.Item1, IdentityFileOptionName, StringComparison.OrdinalIgnoreCase));

                    vm.IdentityFile = identityFileOption?.Item2;
                }
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            string queryString = uri.Query?.Trim();

            if (string.IsNullOrEmpty(queryString))
            {
                return vm;
            }

            if (queryString.StartsWith("?", StringComparison.Ordinal))
            {
                queryString = queryString.Substring(1);
            }

            var queryStringParams = ParseParams(queryString, '&').ToList();

            var moshPorts = queryStringParams.FirstOrDefault(p =>
                ValidMoshPortsNames.Any(n => string.Equals(p.Item1, n, StringComparison.OrdinalIgnoreCase)));

            if (moshPorts != null)
            {
                Match match = MoshRangeRx.Match(moshPorts.Item2);

                if (match.Success)
                {
                    vm.MoshPortFrom = ushort.Parse(match.Groups["from"].Value);
                    vm.MoshPortTo = ushort.Parse(match.Groups["to"].Value);
                }
            }

            vm.LoadBaseFromQueryString(queryStringParams);

            return vm;
        }

        public override async Task<Tuple<bool, string>> GetUrlAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return Tuple.Create(false, error);
            }

            var result = await GetSshInfoValidationResultAsync();

            if (result != SshConnectionInfoValidationResult.Valid &&
                // For links we can ignore missing username
                result != SshConnectionInfoValidationResult.UsernameEmpty)
            {

                error = GetErrorString(result, Environment.NewLine);

                if (string.IsNullOrEmpty(error))
                {
                    error = "Invalid input.";
                }

                return Tuple.Create(false, error);
            }

            StringBuilder sb = new StringBuilder(_useMosh ? MoshUriScheme : SshUriScheme);

            sb.Append("://");

            bool containsUserInfo = false;

            if (!string.IsNullOrEmpty(_username))
            {
                sb.Append(HttpUtility.UrlEncode(_username));

                containsUserInfo = true;
            }

            if (!string.IsNullOrEmpty(_identityFile))
            {
                sb.Append(";");

                if (!string.IsNullOrEmpty(_identityFile))
                {
                    sb.Append($"{IdentityFileOptionName}={HttpUtility.UrlEncode(_identityFile)}");
                }

                containsUserInfo = true;
            }

            if (containsUserInfo)
                sb.Append("@");

            sb.Append(_host);

            if (_sshPort != SshProfile.DefaultSshPort)
            {
                sb.Append($":{_sshPort:#####}");
            }

            sb.Append("/");

            bool queryStringAdded = false;

            if (_useMosh)
            {
                sb.Append(
                    $"?{ValidMoshPortsNames[0]}={_moshPortFrom:#####}-{_moshPortTo:#####}");

                queryStringAdded = true;
            }

            var terminalInfoQueryString = GetBaseQueryString();

            if (!string.IsNullOrEmpty(terminalInfoQueryString))
            {
                sb.Append($"{(queryStringAdded ? "&" : "?")}{terminalInfoQueryString}");
            }

            return Tuple.Create(true, sb.ToString());
        }

        #endregion Links/shortcuts related
    }
}