using System.Text.RegularExpressions;
using Windows.ApplicationModel.Activation;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Protocols
{
    internal static class SshProtocolHandler
    {
        private const ushort DefaultSshPort = 22;

        private const string SshProtocol = "ssh";

        private static readonly Regex SshUrlRx =
            new Regex(
                @"^\s*ssh://(?<user>[^;@]+)(;fingerprint=(?<fingerprint>[^@]+))?@(?<host>[^/:\s]+)(:(?<port>\d{1,5}))?/?\s*$",
                RegexOptions.Compiled);

        internal static bool IsSshProtocol(ProtocolActivatedEventArgs protocolEventArgs) =>
            protocolEventArgs.Uri.Scheme.ToLowerInvariant().Equals(SshProtocol);

        internal static ISshConnectionInfo GetSshConnectionInfo(ProtocolActivatedEventArgs protocolEventArgs)
        {
            Match match = SshUrlRx.Match(protocolEventArgs.Uri.AbsoluteUri);

            return match.Success
                ? new SshConnectionInfoViewModel
                {
                    Host = match.Groups["host"].Value,
                    SshPort = match.Groups["port"].Success ? ushort.Parse(match.Groups["port"].Value) : DefaultSshPort,
                    Username = match.Groups["user"].Value
                }
                : null;
        }

        internal static ShellProfile GetSshShellProfile(ProtocolActivatedEventArgs protocolEventArgs)
        {
            Match match = SshUrlRx.Match(protocolEventArgs.Uri.AbsoluteUri);

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