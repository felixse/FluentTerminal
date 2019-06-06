using FluentTerminal.Models.Enums;
using System.Linq;

namespace FluentTerminal.Models
{
    public class SshProfile : ShellProfile, ISshConnectionInfo
    {

        #region Constants

        public const ushort DefaultSshPort = 22;
        public const ushort DefaultMoshPortsFrom = 60001;
        public const ushort DefaultMoshPortsTo = 60999;

        #endregion Constants

        #region Properties

        public string Host { get; set; }

        public ushort SshPort { get; set; } = DefaultSshPort;

        public string Username { get; set; }

        public string IdentityFile { get; set; }

        public bool UseMosh { get; set; }

        public ushort MoshPortFrom { get; set; } = DefaultMoshPortsFrom;

        public ushort MoshPortTo { get; set; } = DefaultMoshPortsTo;

        #endregion Properties

        #region Constructors

        public SshProfile() { }

        public SshProfile(SshProfile other) : base(other)
        {
            Host = other.Host;
            SshPort = other.SshPort;
            Username = other.Username;
            IdentityFile = other.IdentityFile;
            UseMosh = other.UseMosh;
            MoshPortFrom = other.MoshPortFrom;
            MoshPortTo = other.MoshPortTo;
            TerminalThemeId = other.TerminalThemeId;
            TabThemeId = other.TabThemeId;
            LineEndingTranslation = other.LineEndingTranslation;
            KeyBindings = other.KeyBindings.Select(x => new KeyBinding(x)).ToList();
        }

        #endregion Constructors

        #region Methods

        public override bool EqualTo(ShellProfile other)
        {
            if (!(other is SshProfile otherSsh))
            {
                return false;
            }

            if (ReferenceEquals(this, otherSsh))
            {
                return true;
            }

            return otherSsh.Id.Equals(Id) && string.Equals(otherSsh.Name, Name) && string.Equals(otherSsh.Host, Host) &&
                   otherSsh.SshPort.Equals(SshPort) && string.Equals(otherSsh.Username, Username) &&
                   (string.IsNullOrEmpty(IdentityFile)
                       ? string.IsNullOrEmpty(otherSsh.IdentityFile)
                       : string.Equals(otherSsh.IdentityFile, IdentityFile)) && otherSsh.UseMosh.Equals(UseMosh) &&
                   otherSsh.MoshPortFrom.Equals(MoshPortFrom) && otherSsh.MoshPortTo.Equals(MoshPortTo) &&
                   otherSsh.TabThemeId.Equals(TabThemeId) && otherSsh.TerminalThemeId.Equals(TerminalThemeId) &&
                   otherSsh.LineEndingTranslation == LineEndingTranslation && otherSsh.KeyBindings.SequenceEqual(KeyBindings);
        }

        public SshConnectionInfoValidationResult Validate(bool allowNoUser = false) =>
            this.GetValidationResult(allowNoUser);

        public override string ValidateAndGetErrors() => this.GetValidationResult().GetErrorString();

        #endregion Methods
    }
}