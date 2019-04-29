using System;
using System.Text.RegularExpressions;
using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Protocols
{
    public static class MoshProtocolHandler
    {
        private const ushort DefaultSshPort = 22;

        private const string MoshProtocol = "mosh";

        private static readonly Regex MoshUrlRx =
            new Regex(
                @"^\s*mosh://(?<user>[^;@]+)(;fingerprint=(?<fingerprint>[^@]+))?@(?<host>[^/:\s]+)(:(?<port>\d{1,5}))?/?\?(mosh-ports=(?<moshports>(?<firstport>\d{1,5})-(?<lastport>\d{1,5})))?\s*$",
                RegexOptions.Compiled);

        public static bool IsMoshProtocol(Uri uri) =>
            uri.Scheme.ToLowerInvariant().Equals(MoshProtocol);

        public static ISshConnectionInfo GetMoshConnectionInfo(Uri uri)
        {
            Match match = MoshUrlRx.Match(uri.AbsoluteUri);

            return match.Success
                ? new SshConnectionInfoViewModel
                {
                    Host = match.Groups["host"].Value,
                    SshPort = match.Groups["port"].Success ? ushort.Parse(match.Groups["port"].Value) : DefaultSshPort,
                    Username = match.Groups["user"].Value,
                    UseMosh = true,
                    MoshPorts = match.Groups["moshports"].Success ? match.Groups["moshports"].Value.Replace("-", ":") : "60000:60050",
                    FirstMoshPort = match.Groups["firstport"].Success ? match.Groups["firstport"].Value : "60000",
                    LastMoshPort = match.Groups["lastport"].Success ? match.Groups["lastport"].Value : "60050"
                }
                : null;
        }

        public static ShellProfile GetMoshShellProfile(ISshConnectionInfo connectionInfo, string port, string key, string moshClientPath)
        {
            return new ShellProfile
                {
                    Arguments =
                        $"{connectionInfo.Host} {port}",
                    Location = moshClientPath,
                    WorkingDirectory = string.Empty,
                    LineEndingTranslation = LineEndingStyle.DoNotModify,
                    EnvironmentVariables = { { "MOSH_KEY", key } }
                };
        }
    }
}