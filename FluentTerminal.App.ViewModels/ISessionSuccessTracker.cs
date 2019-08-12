namespace FluentTerminal.App.ViewModels
{
    public interface ISessionSuccessTracker
    {
        void SetSuccessfulSessionStart();

        void SetExitCode(int exitCode);
    }
}