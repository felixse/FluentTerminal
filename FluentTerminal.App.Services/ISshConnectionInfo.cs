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

        string Validate(bool allowNoUser = false);
    }
}
