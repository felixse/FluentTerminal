using System;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services.EventArgs
{
    public class NewWindowRequestedEventArgs : System.EventArgs
    {
        public ShellProfile Profile { get; }

        public NewWindowRequestedEventArgs(ShellProfile profile) =>
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }
}