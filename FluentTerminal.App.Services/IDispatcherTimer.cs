using System;

namespace FluentTerminal.App.Services
{
    public interface IDispatcherTimer
    {
        void Start();

        void Stop();

        TimeSpan Interval { get; set; }
        bool IsEnabled { get; }

        event EventHandler<object> Tick;
    }
}