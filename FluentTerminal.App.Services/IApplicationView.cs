using FluentTerminal.App.Services.EventArgs;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public delegate Task CloseRequestedHandler(object sender, CancelableEventArgs args);

    public interface IApplicationView
    {
        event CloseRequestedHandler CloseRequested;

        string Title { get; set; }

        Task RunOnDispatcherThread(Action action);

        Task<bool> TryClose();

        bool ToggleFullScreen();
    }
}