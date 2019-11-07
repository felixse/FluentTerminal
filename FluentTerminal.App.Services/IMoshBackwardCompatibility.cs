using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    //Created due to changes in CLI arguments introduced in the newest mosh.exe (11/2019).
    //TODO: This interface, as well as its implementation probably should be removed down the road.
    public interface IMoshBackwardCompatibility
    {
        T FixProfile<T>(T profile) where T : ShellProfile;
    }
}