using System;
using System.Text.RegularExpressions;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services.Implementation
{
    // The purpose of this class is to ensure backward compatibility of mosh profiles and command history
    // TODO: This class should be probably deleted in few months or so
    public static class MoshBackwardCompatibility
    {
        private static readonly string MoshCommandExe = $"{Constants.MoshCommandName}.exe";

        private static readonly Regex OldFormatRx = new Regex(@".+\s(?<ports>\d{1,5}:\d{1,5})$", RegexOptions.Compiled);

        private static readonly Regex TargetRx =
            new Regex(@"(^|\s+)(?<user>[^\s@;]+)@(?<host>[^\s@:]+)(:(?<port>\d{1,5}))?\s+", RegexOptions.Compiled);

        private static bool IsMoshProfile(ShellProfile profile) =>
            Constants.MoshCommandName.Equals(profile.Location, StringComparison.OrdinalIgnoreCase) ||
            MoshCommandExe.Equals(profile.Location, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the profile contains obsolete mosh syntax, and changes it if it does.
        /// </summary>
        /// <typeparam name="T">The type of the profile (<see cref="ShellProfile"/> or <see cref="SshProfile"/>).</typeparam>
        /// <param name="profile">The profile. Note that the method doesn't change the input profile at all, but creates and
        /// returns its clone if there's something to change.</param>
        /// <remarks>
        /// The method actually checks if <see cref="ShellProfile.Arguments"/> of the input <paramref name="profile"/> is in
        /// obsolete <c>mosh</c> format <c>[ssh_arguments] [target] [mosh_port_range]</c>, and if it is changes it to the new
        /// format <c>--ssh="[ssh_arguments]" -p [mosh_ports] [target]</c>.
        /// </remarks>
        /// <returns>
        /// If the method detected that the input profile is obsolete mosh profile, it will create its clone with the updated
        /// command line arguments, and return it. Otherwise it will simply return unchanged input profile (the same reference
        /// as the input profile). It allows us to simply check if anything is changed by comparing the input and output
        /// profiles for reference equality.
        /// </returns>
        public static T FixProfile<T>(T profile) where T : ShellProfile
        {
            // If there aren't any arguments, or if the profile isn't a mosh profile, there's nothing to do. Just return.
            if (string.IsNullOrWhiteSpace(profile.Arguments) || !IsMoshProfile(profile)) return profile;

            var arguments = profile.Arguments.Trim();

            var moshPortsMatch = OldFormatRx.Match(arguments);

            if (!moshPortsMatch.Success) return profile;// Arguments don't end with mosh ports (i.e. 60000:61000), so it isn't an obsolete format.

            var targetMatches = TargetRx.Matches(arguments);

            if (targetMatches.Count != 1) return profile;// Target part (user@host:port) not found or found multiple times. Cannot reliably determine the target, so don't change.

            var targetMatch = targetMatches[0];

            // Removing [mosh_ports] from the original arguments
            arguments = arguments.Substring(0, arguments.Length - moshPortsMatch.Groups["ports"].Length).Trim();

            var newProfile = profile.Clone();

            // Removing [target] from the original arguments, thus getting "pure" ssh arguments
            var sshArguments = targetMatch.Index > 0 ? arguments.Substring(0, targetMatch.Index).Trim() : string.Empty;

            if (targetMatch.Index + targetMatch.Length < arguments.Length)
            {
                sshArguments += $" {arguments.Substring(targetMatch.Index + targetMatch.Length).Trim()}";
            }

            // If the ssh port is defined in the original target (i.e. user@host:port), which was allowed in the obsolete mosh, extracting the port part into a separate ssh argument
            if (targetMatch.Groups["port"].Success && uint.TryParse(targetMatch.Groups["port"].Value, out var port) &&
                port != SshProfile.DefaultSshPort)
            {
                sshArguments += $" -p {targetMatch.Groups["port"].Value}";
            }

            // Replacing any double quotes in ssh arguments with single quotes, since ssh arguments will be double-quoted as a whole
            sshArguments = sshArguments.Replace("\"", "'").Trim();

            arguments = $"-p {moshPortsMatch.Groups["ports"].Value}";

            if (!string.IsNullOrEmpty(sshArguments))
            {
                // If there were some ssh arguments, add them as defined in the new format
                arguments += $" --ssh=\"ssh {sshArguments}\"";
            }

            // Finally append the target to the arguments
            arguments += $" {targetMatch.Groups["user"]}@{targetMatch.Groups["host"]}";

            newProfile.Arguments = arguments;

            return (T) newProfile;
        }
    }
}