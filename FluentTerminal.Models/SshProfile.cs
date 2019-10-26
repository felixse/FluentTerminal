using FluentTerminal.Models.Enums;
using System.Linq;
using Windows.Foundation.Metadata;

namespace FluentTerminal.Models
{
    public class SshProfile : ShellProfile
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

        public SshProfile()
        {
            LineEndingTranslation = LineEndingStyle.ToLF;
            UseConPty = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7); // Windows 10 1809+
        }

        public bool RequestConPty
        {
            set
            {
                UseConPty = UseConPty && value;
            }
        }

        protected SshProfile(SshProfile other) : base(other)
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
            if (!(other is SshProfile otherSsh) || !base.EqualTo(other))
            {
                return false;
            }

            if (ReferenceEquals(this, otherSsh))
            {
                return true;
            }

            return otherSsh.Host.NullableEqualTo(Host)
                   && otherSsh.SshPort.Equals(SshPort)
                   && otherSsh.Username.NullableEqualTo(Username)
                   && otherSsh.IdentityFile.NullableEqualTo(IdentityFile)
                   && otherSsh.UseMosh.Equals(UseMosh)
                   && otherSsh.MoshPortFrom.Equals(MoshPortFrom)
                   && otherSsh.MoshPortTo.Equals(MoshPortTo);
        }

        public override ShellProfile Clone() => new SshProfile(this);

        #endregion Methods
    }
}