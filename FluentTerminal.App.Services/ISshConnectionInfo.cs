using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services
{
    public interface ISshConnectionInfo
    {
        string Host { get; set; }

        ushort SshPort { get; set; }

        string Username { get; set; }

        string IdentityFile { get; set; }

        bool UseMosh { get; set; }

        ushort MoshPortFrom { get; set; }

        ushort MoshPortTo { get; set; }

        LineEndingStyle LineEndingStyle { get; set; }

        SshConnectionInfoValidationResult Validate(bool allowNoUser = false);
    }
}