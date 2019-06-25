using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public static class SshConnectionInfoExtensions
    {
        public static SshConnectionInfoValidationResult GetValidationResult(this ISshConnectionInfo sshConnectionInfo)
        {
            var result = SshConnectionInfoValidationResult.Valid;

            if (string.IsNullOrEmpty(sshConnectionInfo.Username))
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

    }
}