using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services
{
    // Resources:
    // https://tools.ietf.org/html/draft-ietf-secsh-scp-sftp-ssh-uri-04#section-3
    // https://man.openbsd.org/ssh
    public class SshHelperService : ISshHelperService
    {
        #region Constants

        private const string SshUriScheme = "ssh";
        private const string MoshUriScheme = "mosh";
        private const string ThemeQueryStringParam = "theme";
        private const string TabQueryStringParam = "tab";

        // Constant derived from https://man.openbsd.org/ssh
        private const string IdentityFileOptionName = "IdentityFile";

        private static readonly string[] ValidMoshPortsNames = { "mosh_ports", "mosh-ports" };

        private static readonly Regex MoshRangeRx =
            new Regex(@"^(?<from>\d{1,5})[:-](?<to>\d{1,5})$", RegexOptions.Compiled);

        #endregion Constants

        #region Static

        private static IEnumerable<Tuple<string, string>> ParseParams(string uriOpts, char separator) =>
            uriOpts.Split(separator).Select(ParseSshOptionFromUri).Where(p => p != null);

        private static Tuple<string, string> ParseSshOptionFromUri(string option)
        {
            string[] nv = option.Split('=');

            if (nv.Length != 2 || string.IsNullOrEmpty(nv[0]))
            {
                // For now simply ignore invalid options
                return null;
                //throw new FormatException($"Invalid SSH option '{option}'.");
            }

            return Tuple.Create(HttpUtility.UrlDecode(nv[0]), HttpUtility.UrlDecode(nv[1]));
        }

        #endregion Static

        #region Fields

        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystemService;
        private readonly IApplicationView _applicationView;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        #endregion Fields

        #region Constructor

        public SshHelperService(ISettingsService settingsService, IDialogService dialogService,
            IFileSystemService fileSystemService, IApplicationView applicationView,
            IDefaultValueProvider defaultValueProvider,
            ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;
            _defaultValueProvider = defaultValueProvider;
            _trayProcessCommunicationService = trayProcessCommunicationService;
        }

        #endregion Constructor

        #region Methods

        public bool IsSsh(Uri uri) =>
            SshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase) ||
            MoshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase);

        public ISshConnectionInfo ParseSsh(Uri uri)
        {
            SshProfileViewModel vm = new SshProfileViewModel(null, _settingsService, _dialogService,
                _fileSystemService, _applicationView, _defaultValueProvider, _trayProcessCommunicationService, true)
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

            if (string.IsNullOrEmpty(uri.Query))
            {
                return vm;
            }

            string queryString = uri.Query?.Trim();

            if (string.IsNullOrEmpty(queryString))
            {
                return vm;
            }

            if (queryString.StartsWith('?'))
            {
                queryString = queryString.Substring(1);
            }

            foreach (Tuple<string, string> param in ParseParams(queryString, '&'))
            {
                string paramName = param.Item1.ToLower();

                if (ValidMoshPortsNames.Contains(paramName))
                {
                    Match match = MoshRangeRx.Match(param.Item2);

                    if (match.Success)
                    {
                        vm.MoshPortFrom = ushort.Parse(match.Groups["from"].Value);
                        vm.MoshPortTo = ushort.Parse(match.Groups["to"].Value);
                    }

                    continue;
                }

                if (param.Item1.Equals(ThemeQueryStringParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (Guid.TryParse(param.Item2, out Guid themeId))
                    {
                        vm.TerminalThemeId = themeId;
                    }

                    continue;
                }

                if (param.Item2.Equals(TabQueryStringParam, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(param.Item2, out int tabThemeId))
                    {
                        vm.TabThemeId = tabThemeId;
                    }
                }

                // For now we are ignoring unknown query string arguments
            }

            return vm;
        }

        public async Task<SshProfile> GetSshProfileAsync(SshProfile profile)
        {
            SshProfileViewModel vm = new SshProfileViewModel(profile, _settingsService, _dialogService,
                _fileSystemService, _applicationView, _defaultValueProvider, _trayProcessCommunicationService, true);

            vm.UseMosh = _settingsService.GetApplicationSettings().UseMoshByDefault;

            vm = (SshProfileViewModel) await _dialogService.ShowSshConnectionInfoDialogAsync(vm);

            return (SshProfile) vm?.Model;
        }

        public Task<SshProfile> GetSavedSshProfileAsync() =>
            _dialogService.ShowSshProfileSelectionDialogAsync();

        public async Task<string> ConvertToUriAsync(ISshConnectionInfo sshConnectionInfo)
        {
            var result = await sshConnectionInfo.ValidateAsync();

            if (result != SshConnectionInfoValidationResult.Valid &&
                // We can ignore empty username here
                result != SshConnectionInfoValidationResult.UsernameEmpty)
                throw new ArgumentException(result.GetErrorString(), nameof(sshConnectionInfo));

            SshProfileViewModel sshConnectionInfoVm = (SshProfileViewModel) sshConnectionInfo;

            StringBuilder sb = new StringBuilder(sshConnectionInfoVm.UseMosh ? MoshUriScheme : SshUriScheme);

            sb.Append("://");

            bool containsUserInfo = false;

            if (!string.IsNullOrEmpty(sshConnectionInfoVm.Username))
            {
                sb.Append(HttpUtility.UrlEncode(sshConnectionInfoVm.Username));

                containsUserInfo = true;
            }

            if (!string.IsNullOrEmpty(sshConnectionInfoVm.IdentityFile))
            {
                sb.Append(";");

                if (!string.IsNullOrEmpty(sshConnectionInfoVm.IdentityFile))
                {
                    sb.Append($"{IdentityFileOptionName}={HttpUtility.UrlEncode(sshConnectionInfoVm.IdentityFile)}");
                }

                containsUserInfo = true;
            }

            if (containsUserInfo)
                sb.Append("@");

            sb.Append(sshConnectionInfoVm.Host);

            if (sshConnectionInfoVm.SshPort != SshProfile.DefaultSshPort)
            {
                sb.Append(":");
                sb.Append(sshConnectionInfoVm.SshPort.ToString("#####"));
            }

            sb.Append("/");

            bool queryStringAdded = false;

            if (sshConnectionInfoVm.UseMosh)
            {
                sb.Append(
                    $"?{ValidMoshPortsNames[0]}={sshConnectionInfo.MoshPortFrom:#####}-{sshConnectionInfo.MoshPortTo:#####}");

                queryStringAdded = true;
            }

            if (!sshConnectionInfo.TerminalThemeId.Equals(Guid.Empty))
            {
                if (!queryStringAdded)
                {
                    sb.Append("?");

                    queryStringAdded = true;
                }
                else
                {
                    sb.Append("&");
                }

                sb.Append($"{ThemeQueryStringParam}={sshConnectionInfo.TerminalThemeId}" );
            }

            if (sshConnectionInfo.TabThemeId != 0)
            {
                if (!queryStringAdded)
                {
                    sb.Append("?");
                }
                else
                {
                    sb.Append("&");
                }

                sb.Append($"{TabQueryStringParam}={sshConnectionInfo.TabThemeId:##########}");
            }

            return sb.ToString();
        }

        #endregion Methods
    }
}