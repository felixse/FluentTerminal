using FluentTerminal.App.Services.Dialogs;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    public class SshConnectionInfoViewModel : ViewModelBase, ISshConnectionInfo
    {
        private string _host = string.Empty;

        public string Host
        {
            get => _host;
            set => Set(ref _host, value);
        }

        private ushort _port = 22;

        public ushort Port
        {
            get => _port;
            set => Set(ref _port, value);
        }

        private string _username = string.Empty;

        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
        }
    }
}