using System;
using System.Collections.Generic;
using System.Linq;
using FluentTerminal.Models.Enums;
using Windows.ApplicationModel.Resources;

namespace FluentTerminal.Models
{
    public static class Extensions
    {
        public static string GetResource(this string resource) =>
            ResourceLoader.GetForCurrentView().GetString(resource.Replace('.', '/'));

        public static SshConnectionInfoValidationResult GetValidationResult(this ISshConnectionInfo sshConnectionInfo,
            bool allowNoUser = false)
        {
            var result = SshConnectionInfoValidationResult.Valid;

            if (!allowNoUser && string.IsNullOrEmpty(sshConnectionInfo.Username))
            {
                result |= SshConnectionInfoValidationResult.UsernameEmpty;
            }

            if (string.IsNullOrEmpty(sshConnectionInfo.Host))
            {
                result |= SshConnectionInfoValidationResult.HostEmpty;
            }

            if (sshConnectionInfo.SshPort < 1)
            {
                result |= SshConnectionInfoValidationResult.SshPortZeroOrNegative;
            }

            if (!sshConnectionInfo.UseMosh)
            {
                return result;
            }

            if (sshConnectionInfo.MoshPortFrom < 1)
            {
                result |= SshConnectionInfoValidationResult.MoshPortZeroOrNegative;
            }

            if (sshConnectionInfo.MoshPortFrom > sshConnectionInfo.MoshPortTo)
            {
                result |= SshConnectionInfoValidationResult.MoshPortRangeInvalid;
            }

            return result;
        }

        public static string GetErrorString(this SshConnectionInfoValidationResult result, string separator = "; ") =>
            string.Join(separator, result.GetErrors());

        public static IEnumerable<string> GetErrors(this SshConnectionInfoValidationResult result)
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
                    yield return $"{nameof(SshConnectionInfoValidationResult)}.{value}".GetResource();
                }
            }
        }
    }
}