namespace FluentTerminal.App.ViewModels
{
    public interface ISessionSuccessTracker
    {
        void SetSuccessfulSessionStart();

        void SetOutputReceived();

        void SetInvalid();
    }
}