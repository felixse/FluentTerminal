using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace FluentTerminal.App.ViewModels
{
    public class EnvironmentVariableViewModel : ObservableObject
    {
        private string _name;
        private string _value;

        public Guid Id { get; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
