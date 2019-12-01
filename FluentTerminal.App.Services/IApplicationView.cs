using FluentTerminal.App.Services.EventArgs;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace FluentTerminal.App.Services
{
    public delegate Task CloseRequestedHandler(object sender, CancelableEventArgs args);

    public interface IApplicationView
    {
        event CloseRequestedHandler CloseRequested;
        event EventHandler Closed;

        int Id { get; }
        string Title { get; set; }

        Task DispatchAsync(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal,
            bool enforceNewSchedule = false);

        Task<T> DispatchAsync<T>(Func<T> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal,
            bool enforceNewSchedule = false);

        Task<bool> TryClose();

        bool ToggleFullScreen();
        bool IsApiContractPresent(string api, ushort version);
    }
}