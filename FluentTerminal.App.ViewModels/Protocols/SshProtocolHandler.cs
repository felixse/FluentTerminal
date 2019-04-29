using System;
using System.Text.RegularExpressions;
using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Protocols
{
    public static class SshProtocolHandler
    {
        private const ushort DefaultSshPort = 22;

        private const string SshProtocol = "ssh";

        private static readonly Regex SshUrlRx =
            new Regex(
                @"^\s*ssh://(?<user>[^;@]+)(;fingerprint=(?<fingerprint>[^@]+))?@(?<host>[^/:\s]+)(:(?<port>\d{1,5}))?/?\s*$",
                RegexOptions.Compiled);

        public static bool IsSshProtocol(Uri uri) =>
            uri.Scheme.ToLowerInvariant().Equals(SshProtocol);

        public static ISshConnectionInfo GetSshConnectionInfo(Uri uri)
        {
            Match match = SshUrlRx.Match(uri.AbsoluteUri);

            return match.Success
                ? new SshConnectionInfoViewModel
                {
                    Host = match.Groups["host"].Value,
                    SshPort = match.Groups["port"].Success ? ushort.Parse(match.Groups["port"].Value) : DefaultSshPort,
                    Username = match.Groups["user"].Value
                }
                : null;
        }

        public static ShellProfile GetSshShellProfile(Uri uri)
        {
            Match match = SshUrlRx.Match(uri.AbsoluteUri);

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