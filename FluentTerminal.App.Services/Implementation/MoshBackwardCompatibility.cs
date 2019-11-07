using System;
using System.Text.RegularExpressions;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services.Implementation
{
    public class MoshBackwardCompatibility : IMoshBackwardCompatibility
    {
        private static readonly string MoshCommandExe = $"{Constants.MoshCommandName}.exe";

        private static readonly Regex OldFormatRx = new Regex(@".+\s(?<ports>\d{1,5}:\d{1,5})$", RegexOptions.Compiled);

        private static readonly Regex TargetRx =
            new Regex(@"(^|\s+)(?<user>[^\s@;]+)@(?<host>[^\s@:]+)(:(?<port>\d{1,5}))?\s+", RegexOptions.Compiled);

        public ShellProfile FixProfile(ShellProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Arguments) || !IsMoshProfile(profile)) return profile;

            var arguments = profile.Arguments.Trim();

            var moshPortsMatch = OldFormatRx.Match(arguments);

            if (!moshPortsMatch.Success) return profile;// Arguments don't end with mosh ports (i.e. 60000:61000)

            var targetMatches = TargetRx.Matches(arguments);

            if (targetMatches.Count != 1) return profile;// Target part (user@host:port) not found or found multiple times

            var targetMatch = targetMatches[0];

            arguments = arguments.Substring(0, arguments.Length - moshPortsMatch.Groups["ports"].Length).Trim();

            var newProfile = profile.Clone();

            var sshArguments = targetMatch.Index > 0 ? arguments.Substring(0, targetMatch.Index).Trim() : string.Empty;

            if (targetMatch.Index + targetMatch.Length < arguments.Length)
            {
                sshArguments += $" {arguments.Substring(targetMatch.Index + targetMatch.Length).Trim()}";
            }

            if (targetMatch.Groups["port"].Success && uint.TryParse(targetMatch.Groups["port"].Value, out var port) &&
                port != SshProfile.DefaultSshPort)
            {
                sshArguments += $" -p {targetMatch.Groups["port"].Value}";
            }

            sshArguments = sshArguments.Replace("\"", "'").Trim();

            arguments = $"-p {moshPortsMatch.Groups["ports"].Value}";

            if (!string.IsNullOrEmpty(sshArguments))
            {
                arguments += $"--ssh=\"{sshArguments}\"";
            }

            arguments += $" {targetMatch.Groups["user"]}@{targetMatch.Groups["host"]}";

            newProfile.Arguments = arguments;

            return newProfile;
        }

        private bool IsMoshProfile(ShellProfile profile) =>
            Constants.MoshCommandName.Equals(profile.Location, StringComparison.OrdinalIgnoreCase) ||
            MoshCommandExe.Equals(profile.Location, StringComparison.OrdinalIgnoreCase);
    }
}