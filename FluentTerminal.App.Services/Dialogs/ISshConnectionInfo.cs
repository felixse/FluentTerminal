namespace FluentTerminal.App.Services.Dialogs
{
    public interface ISshConnectionInfo
    {
        string Host { get; set; }

        ushort Port { get; set; }

        string Username { get; set; }
    }
}