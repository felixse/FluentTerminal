using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.ViewModels
{
    public class ProfileCommandViewModel : ViewModelBase
    {
        public ShellProfile Profile { get; }

        public RelayCommand Command { get; }

        public ProfileCommandViewModel(ShellProfile profile, RelayCommand command)
        {
            Profile = profile;
            Command = command;
        }
    }
}