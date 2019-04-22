using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    public class SshOptionViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _value;

        public string Value
        {
            get => _value;
            set => Set(ref _value, value);
        }
    }
}
