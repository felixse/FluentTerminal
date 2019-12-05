using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace FluentTerminal.App.Utilities
{
    internal static class DispatcherExtensions
    {
        internal static Task ExecuteAsync(this CoreDispatcher dispatcher, Action action,
            CoreDispatcherPriority priority = CoreDispatcherPriority.Normal, bool enforceNewSchedule = false)
        {
            if (enforceNewSchedule || !dispatcher.HasThreadAccess)
            {
                return dispatcher.RunAsync(priority, () => action()).AsTask();
            }

            action();

            return Task.CompletedTask;
        }

        internal static Task<T> ExecuteAsync<T>(this CoreDispatcher dispatcher, Func<T> func,
            CoreDispatcherPriority priority = CoreDispatcherPriority.Normal, bool enforceNewSchedule = false)
        {
            if (!enforceNewSchedule && dispatcher.HasThreadAccess)
            {
                return Task.FromResult(func());
            }

            var tcs = new TaskCompletionSource<T>();

            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = dispatcher.RunAsync(priority, () =>
            {
                T val;

                try
                {
                    val = func();
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);

                    return;
                }

                tcs.TrySetResult(val);
            });

            return tcs.Task;
        }
    }
}