using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private static readonly Regex CommandValidationRx = new Regex(@"^(?<cmd>[^\s\.]+)(\.exe)?(\s+(?<args>\S.*))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] AcceptableCommands = { Constants.SshCommandName };

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
        private readonly IApplicationDataContainer _historyContainer;

        private bool _commandInputOriginal;

        // To prevent validating the existence of the same file multiple times because it's kinda expensive
        private string _validatedIdentityFile;

        #endregion Fields

        #region Properties

        private bool _commandInput;

        public bool CommandInput
        {
            get => _commandInput;
            set
            {
                if (Set(ref _commandInput, value) && _useMosh)
                {
                    RaisePropertyChanged(nameof(MoshVisible));
                }
            }
        }

        #region Quick SSH properties

        private string _command;

        public string Command
        {
            get => _command;
            set => Set(ref _command, value);
        }

        #endregion Quick SSH properties

        #region Full UI properties

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
            set
            {
                if (Set(ref _useMosh, value) && !_commandInput)
                {
                    RaisePropertyChanged(nameof(MoshVisible));
                }
            }
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

        #endregion Full UI properties

        public bool MoshVisible => _useMosh && !_commandInput;

        public ObservableCollection<string> CommandHistory { get; private set; }

        #endregion Properties

        #region Commands

        public IAsyncCommand BrowseForIdentityFileCommand { get; }

        #endregion Commands

        #region Constructors

        public SshConnectViewModel(ISettingsService settingsService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, IFileSystemService fileSystemService,
            IApplicationDataContainer historyContainer, SshProfile original = null) : base(settingsService, applicationView,
            original ?? new SshProfile { UseMosh = settingsService.GetApplicationSettings().UseMoshByDefault })
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _fileSystemService = fileSystemService;
            _historyContainer = historyContainer;

            if (string.IsNullOrEmpty(Model.Location))
            {
                _commandInput = settingsService.GetApplicationSettings().UseQuickSshConnectByDefault;
            }
            else if (string.IsNullOrEmpty(((SshProfile)Model).Host))
            {
                _commandInput = true;
            }
            else
            {
                _commandInput = false;
            }

            _commandInputOriginal = _commandInput;

            FillCommandHistory();

            Initialize((SshProfile)Model);

            BrowseForIdentityFileCommand = new AsyncCommand(BrowseForIdentityFile);
        }

        private SshConnectViewModel(ISettingsService settingsService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, IFileSystemService fileSystemService,
            IApplicationDataContainer historyContainer, bool useCommandInput) : base(settingsService, applicationView,
            new SshProfile { UseMosh = settingsService.GetApplicationSettings().UseMoshByDefault })
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _fileSystemService = fileSystemService;
            _historyContainer = historyContainer;
            _commandInput = useCommandInput;
            _commandInputOriginal = useCommandInput;

            FillCommandHistory();

            Initialize((SshProfile)Model);

            BrowseForIdentityFileCommand = new AsyncCommand(BrowseForIdentityFile);
        }

        #endregion Constructors

        #region Methods

        // Fills the view model properties from the input sshProfile
        private void Initialize(SshProfile sshProfile)
        {
            Command = string.IsNullOrEmpty(sshProfile.Location) ? string.Empty : $"{sshProfile.Location} {sshProfile.Arguments}";

            Host = sshProfile.Host;
            SshPort = sshProfile.SshPort;

            Username = sshProfile.Username;

            if (string.IsNullOrEmpty(Username))
            {
                _trayProcessCommunicationService.GetUserName().ContinueWith(t =>
                {
                    var username = t.Result;

                    if (string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(username))
                    {
                        ApplicationView.RunOnDispatcherThread(() => Username = username, false);
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

        protected override void LoadFromProfile(ShellProfile profile)
        {
            base.LoadFromProfile(profile);

            CommandInput = _commandInputOriginal;

            Initialize((SshProfile)profile);
        }

        protected override void CopyToProfile(ShellProfile profile)
        {
            base.CopyToProfile(profile);

            _commandInputOriginal = _commandInput;

            if (_commandInput)
            {
                CopyToProfileQuick((SshProfile)profile);
            }
            else
            {
                CopyToProfileFull((SshProfile)profile);
            }
        }

        private void CopyToProfileFull(SshProfile sshProfile)
        {
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

        private void CopyToProfileQuick(SshProfile sshProfile)
        {
            var command = _command?.Trim();

            if (string.IsNullOrEmpty(command))
            {
                sshProfile.Location = null;
                sshProfile.Arguments = null;

                return;
            }

            var match = CommandValidationRx.Match(command);

            if (!match.Success)
            {
                // Should not happen ever because this method gets called only if validation succeeds.
                throw new Exception("Invalid command.");
            }

            sshProfile.Location = match.Groups["cmd"].Value;
            sshProfile.Arguments = match.Groups["args"].Success ? match.Groups["args"].Value.Trim() : null;

            sshProfile.WorkingDirectory = null;

            sshProfile.Host = null;
            sshProfile.SshPort = 0;
            sshProfile.Username = null;
            sshProfile.IdentityFile = null;
            sshProfile.UseMosh = false;
            sshProfile.MoshPortFrom = 0;
            sshProfile.MoshPortTo = 0;
        }

        public override Task<string> ValidateAsync()
        {
            return _commandInput ? ValidateQuickAsync() : ValidateFullAsync();
        }

        private async Task<string> ValidateFullAsync()
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

        private async Task<string> ValidateQuickAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            var match = CommandValidationRx.Match(_command?.Trim() ?? string.Empty);

            if (!match.Success)
            {
                error = I18N.Translate("InvalidCommand");

                return string.IsNullOrEmpty(error) ? "Invalid command." : error;
            }

            if (!AcceptableCommands.Any(c => c.Equals(match.Groups["cmd"].Value, StringComparison.OrdinalIgnoreCase)))
            {
                error = I18N.Translate("UnsupportedCommand");

                return string.IsNullOrEmpty(error)
                    ? $"Unsupported command: {match.Groups["cmd"]}."
                    : $"{error} {match.Groups["cmd"]}";
            }

            if (!match.Groups["args"].Success)
            {
                error = I18N.Translate("CommandArgumentsMandatory");

                return string.IsNullOrEmpty(error) ? "Command arguments are missing." : error;
            }

            return null;
        }

        public override bool HasChanges()
        {
            // ReSharper disable once ArrangeRedundantParentheses
            if (base.HasChanges() || (_commandInput != _commandInputOriginal))
            {
                return true;
            }

            if (_commandInput)
            {
                return !_command.NullableEqualTo($"{Model.Location} {Model.Arguments}");
            }

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

        #region Command history

        private const string ExecutedCommandsKey = "ExecutedCommands";
        private const int CommandHistoryLimit = 20;

        private void FillCommandHistory()
        {
            var commands = _historyContainer.ReadValueFromJson<ExecutedCommandHistory>(ExecutedCommandsKey, null)
                ?.ExecutedCommands;

            CommandHistory = commands == null
                ? new ObservableCollection<string>()
                : new ObservableCollection<string>(commands.Select(c => $"{c.Command} {c.Args}"));
        }

        public void SaveCommand(string cmd, string args)
        {
            cmd = cmd?.Trim();

            if (string.IsNullOrEmpty(cmd))
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            var commandHistory =
                _historyContainer.ReadValueFromJson<ExecutedCommandHistory>(ExecutedCommandsKey, null) ??
                new ExecutedCommandHistory { ExecutedCommands = new List<ExecutedCommand>() };

            var command = commandHistory.ExecutedCommands.FirstOrDefault(c =>
                string.Equals(cmd, c.Command, StringComparison.Ordinal) && args.NullableEqualTo(c.Args));

            if (command == null)
            {
                command = new ExecutedCommand
                { Command = cmd, Args = args, ExecutionCount = 1, LastExecution = DateTime.UtcNow };
            }
            else
            {
                command.ExecutionCount++;

                command.LastExecution = DateTime.UtcNow;

                commandHistory.ExecutedCommands.Remove(command);
            }

            commandHistory.ExecutedCommands.Insert(0, command);

            while (commandHistory.ExecutedCommands.Count > CommandHistoryLimit)
            {
                commandHistory.ExecutedCommands.RemoveAt(CommandHistoryLimit);
            }

            _historyContainer.WriteValueAsJson(ExecutedCommandsKey, commandHistory);
        }

        #endregion Command history

        #region Links/shortcuts related

        // Resources:
        // https://tools.ietf.org/html/draft-ietf-secsh-scp-sftp-ssh-uri-04#section-3
        // https://man.openbsd.org/ssh

        private const string SshUriScheme = "ssh";
        private const string MoshUriScheme = "mosh";

        private const string CustomSshUriScheme = "sshft";
        private const string CustomUriHost = "fluent.terminal";
        private const string CustomCommandQueryStringName = "cmd";
        private const string CustomArgumentsQueryStringName = "args";

        // Constant derived from https://man.openbsd.org/ssh
        private const string IdentityFileOptionName = "IdentityFile";

        private static readonly string[] ValidMoshPortsNames = { "mosh_ports", "mosh-ports" };

        private static readonly Regex MoshRangeRx =
            new Regex(@"^(?<from>\d{1,5})[:-](?<to>\d{1,5})$", RegexOptions.Compiled);

        public static bool CheckScheme(Uri uri) => CheckSchemeStandard(uri) || CheckSchemeCustom(uri);

        private static bool CheckSchemeStandard(Uri uri) =>
            SshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase) ||
            MoshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase);

        private static bool CheckSchemeCustom(Uri uri) =>
            CustomSshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase);

        public static SshConnectViewModel ParseUri(Uri uri, ISettingsService settingsService,
            IApplicationView applicationView, ITrayProcessCommunicationService trayProcessCommunicationService,
            IFileSystemService fileSystemService, IApplicationDataContainer historyContainer)
        {
            if (CheckSchemeStandard(uri))
            {
                return ParseUriStandard(uri, settingsService, applicationView, trayProcessCommunicationService,
                    fileSystemService, historyContainer);
            }

            if (CheckSchemeCustom(uri))
            {
                return ParseUriCustom(uri, settingsService, applicationView, trayProcessCommunicationService,
                    fileSystemService, historyContainer);
            }

            // Won't happen ever
            throw new Exception($"Unsupported URI scheme: {uri.Scheme}");
        }

        private static SshConnectViewModel ParseUriStandard(Uri uri, ISettingsService settingsService,
            IApplicationView applicationView, ITrayProcessCommunicationService trayProcessCommunicationService,
            IFileSystemService fileSystemService, IApplicationDataContainer historyContainer)
        {
            var vm = new SshConnectViewModel(settingsService, applicationView, trayProcessCommunicationService,
                fileSystemService, historyContainer, false)
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

        private static SshConnectViewModel ParseUriCustom(Uri uri, ISettingsService settingsService,
            IApplicationView applicationView, ITrayProcessCommunicationService trayProcessCommunicationService,
            IFileSystemService fileSystemService, IApplicationDataContainer historyContainer)
        {
            var vm = new SshConnectViewModel(settingsService, applicationView, trayProcessCommunicationService,
                fileSystemService, historyContainer, true);

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

            var cmdParam = queryStringParams.FirstOrDefault(t =>
                CustomCommandQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            var argsParam = queryStringParams.FirstOrDefault(t =>
                CustomArgumentsQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            vm._command =
                $"{(string.IsNullOrEmpty(cmdParam?.Item2) ? Constants.SshCommandName : cmdParam.Item2)} {argsParam?.Item2}"
                    .Trim();

            vm.LoadBaseFromQueryString(queryStringParams);

            return vm;
        }

        public override Task<Tuple<bool, string>> GetUrlAsync()
        {
            return _commandInput ? GetUrlQuickAsync() : GetUrlFullAsync();
        }

        private async Task<Tuple<bool, string>> GetUrlFullAsync()
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

        private async Task<Tuple<bool, string>> GetUrlQuickAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return Tuple.Create(false, error);
            }

            var match = CommandValidationRx.Match(_command);

            if (!match.Success)
            {
                error = I18N.Translate("InvalidCommand");

                return Tuple.Create(false, string.IsNullOrEmpty(error) ? "Invalid command." : error);
            }

            return Tuple.Create(true,
                $"{CustomSshUriScheme}://{CustomUriHost}/?{CustomCommandQueryStringName}={Constants.SshCommandName}&{CustomArgumentsQueryStringName}={HttpUtility.UrlEncode(match.Groups["args"].Value.Trim())}&{GetBaseQueryString()}");
        }

        #endregion Links/shortcuts related
    }
}