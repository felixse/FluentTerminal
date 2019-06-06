using System;
using FluentTerminal.Models.Enums;


namespace FluentTerminal.Models
{
    public interface ISshConnectionInfo
    {
        string Host { get; set; }

        ushort SshPort { get; set; }

        string Username { get; set; }

        string IdentityFile { get; set; }

        LineEndingStyle LineEndingTranslation { get; set; }

        bool UseMosh { get; set; }

        ushort MoshPortFrom { get; set; }

        ushort MoshPortTo { get; set; }

        Guid TerminalThemeId { get; set; }

        int TabThemeId { get; set; }

        SshConnectionInfoValidationResult Validate(bool allowNoUser = false);

        bool UseConPty { get; set; }
    }
}
