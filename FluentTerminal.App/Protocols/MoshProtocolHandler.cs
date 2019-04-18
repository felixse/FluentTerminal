using System.Text.RegularExpressions;
using Windows.ApplicationModel.Activation;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Protocols
{
    internal static class MoshProtocolHandler
    {
        private const ushort DefaultSshPort = 22;

        private const string MoshProtocol = "mosh";

        private static readonly Regex MoshUrlRx =
            new Regex(
                @"^\s*mosh://(?<user>[^;@]+)(;fingerprint=(?<fingerprint>[^@]+))?@(?<host>[^/:\s]+)(:(?<port>\d{1,5}))?/?\?(mosh-ports=(?<moshports>(?<firstport>\d{1,5})-(?<lastport>\d{1,5})))?\s*$",
                RegexOptions.Compiled);

        internal static bool IsMoshProtocol(ProtocolActivatedEventArgs protocolEventArgs) =>
            protocolEventArgs.Uri.Scheme.ToLowerInvariant().Equals(MoshProtocol);

        internal static ISshConnectionInfo GetMoshConnectionInfo(ProtocolActivatedEventArgs protocolEventArgs)
        {
            Match match = MoshUrlRx.Match(protocolEventArgs.Uri.AbsoluteUri);

            return match.Success
                ? new SshConnectionInfoViewModel
                {
                    Host = match.Groups["host"].Value,
                    SshPort = match.Groups["port"].Success ? ushort.Parse(match.Groups["port"].Value) : DefaultSshPort,
                    Username = match.Groups["user"].Value,
                    UseMosh = true,
                    MoshPorts = match.Groups["moshports"].Success ? match.Groups["moshports"].Value : "60000-60050",
                    FirstMoshPort = match.Groups["firstport"].Success ? match.Groups["firstport"].Value : "60000",
                    LastMoshPort = match.Groups["lastport"].Success ? match.Groups["lastport"].Value : "60050"
                }
                : null;
        }

        internal static ShellProfile GetSshShellProfile(ProtocolActivatedEventArgs protocolEventArgs)
        {
            Match match = MoshUrlRx.Match(protocolEventArgs.Uri.AbsoluteUri);

            return match.Success
                ? new ShellProfile
                {
                    Arguments =
                        $"-p {(match.Groups["port"].Success ? ushort.Parse(match.Groups["port"].Value) : DefaultSshPort):#####} {match.Groups["user"].Value}@{match.Groups["host"].Value}",
                    Location = @"C:\Windows\System32\OpenSSH\ssh.exe",
                    WorkingDirectory = string.Empty,
                    LineEndingTranslation = LineEndingStyle.DoNotModify
                }
                : null;
        }
    }
}