using System.Collections.ObjectModel;
using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.ViewModels
{
    public class SshConnectionInfoViewModel : ViewModelBase, ISshConnectionInfo
    {
        public const ushort DefaultSshPort = 22;
        public const ushort DefaultMoshPortsFrom = 60000;
        public const ushort DefaultMoshPortsTo = 60050;

        private string _host = string.Empty;

        public string Host
        {
            get => _host;
            set => Set(ref _host, value);
        }

        private ushort _sshPort = DefaultSshPort;

        public ushort SshPort
        {
            get => _sshPort;
            set => Set(ref _sshPort, value);
        }

        private string _username = string.Empty;

        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
        }

        private string _identityFile = string.Empty;

        public string IdentityFile
        {
            get => _identityFile;
            set => Set(ref _identityFile, value);
        }

        private bool _useMosh;

        public bool UseMosh
        {
            get => _useMosh;
            set => Set(ref _useMosh, value);
        }

        private ushort _moshPortFrom = DefaultMoshPortsFrom;

        public ushort MoshPortFrom
        {
            get => _moshPortFrom;
            set => Set(ref _moshPortFrom, value);
        }

        private ushort _moshPortTo = DefaultMoshPortsTo;

        public ushort MoshPortTo
        {
            get => _moshPortTo;
            set => Set(ref _moshPortTo, value);
        }

        private LineEndingStyle _lineEndingStyle = LineEndingStyle.ToLF;
        public LineEndingStyle LineEndingStyle
        {
            get => _lineEndingStyle;
            set => Set(ref _lineEndingStyle, value);
        }

        public bool DoNotModifyIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.DoNotModify;
            set { if (value) LineEndingStyle = LineEndingStyle.DoNotModify; }
        }

        public bool ToCRLFIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.ToCRLF;
            set { if (value) LineEndingStyle = LineEndingStyle.ToCRLF; }
        }

        public bool ToLFIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.ToLF;
            set { if (value) LineEndingStyle = LineEndingStyle.ToLF; }
        }

        public bool ToCRIsSelected
        {
            get => LineEndingStyle == LineEndingStyle.ToCR;
            set { if (value) LineEndingStyle = LineEndingStyle.ToCR; }
        }

        public ObservableCollection<SshOptionViewModel> SshOptions { get; } =
            new ObservableCollection<SshOptionViewModel>();

        public string Validate(bool allowNoUser = false)
        {
            if (!allowNoUser && string.IsNullOrEmpty(_username))
                return "Username cannot be empty.";

            if (string.IsNullOrEmpty(_host))
                return "Host cannot be empty.";

            if (_sshPort < 1)
                return "SSH port has to be greater than zero.";

            if (!_useMosh)
                return null;

            if (_moshPortFrom < 1)
                return "Mosh port cannot be zero.";

            if (_moshPortFrom > _moshPortTo)
                return "Mosh port range is invalid.";

            return null;
        }

        public SshConnectionInfoViewModel Clone()
        {
            SshConnectionInfoViewModel clone = new SshConnectionInfoViewModel
            {
                _host = _host, _sshPort = _sshPort, _username = _username, _identityFile = _identityFile,
                _useMosh = _useMosh, _moshPortFrom = _moshPortFrom, _moshPortTo = _moshPortTo
            };

            foreach (SshOptionViewModel option in SshOptions)
                clone.SshOptions.Add(option);

            return clone;
        }
    }
}