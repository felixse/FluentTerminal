using GalaSoft.MvvmLight;
using System;

namespace FluentTerminal.App.ViewModels
{
    public class EnvironmentVariableViewModel : ViewModelBase
    {
        private string _name;
        private string _value;

        public Guid Id { get; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string Value
        {
            get => _value;
            set => Set(ref _value, value);
        }
    }
}
