using FluentTerminal.Models.Enums;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace FluentTerminal.App.Services
{
    public class StartupTaskService : IStartupTaskService
    {
        private const string StartupTaskName = "FluentTerminalStartupTask";

        public Task<StartupTaskStatus> GetStatusAsync()
        {
            return StartupTask.GetAsync(StartupTaskName).AsTask().ContinueWith(t => ToStartupTaskStatus(t.Result.State),
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<StartupTaskStatus> EnableStartupTaskAsync()
        {
            var startupTask = await StartupTask.GetAsync(StartupTaskName);
            var newState = await startupTask.RequestEnableAsync();
            return ToStartupTaskStatus(newState);
        }

        public Task DisableStartupTaskAsync()
        {
            return StartupTask.GetAsync(StartupTaskName).AsTask().ContinueWith(t => t.Result.Disable(),
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private StartupTaskStatus ToStartupTaskStatus(StartupTaskState state)
        {
            return state switch
            {
                StartupTaskState.Disabled => StartupTaskStatus.Disabled,
                StartupTaskState.DisabledByUser => StartupTaskStatus.DisabledByUser,
                StartupTaskState.Enabled => StartupTaskStatus.Enabled,
                StartupTaskState.DisabledByPolicy => StartupTaskStatus.DisabledByPolicy,
                StartupTaskState.EnabledByPolicy => StartupTaskStatus.EnabledByPolicy,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}
