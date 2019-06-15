using FluentTerminal.Models.Enums;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace FluentTerminal.App.Services
{
    public class StartupTaskService : IStartupTaskService
    {
        public const string StartupTaskname = "FluentTerminalStartupTask";

        public async Task<StartupTaskStatus> GetStatus()
        {
            var startupTask = await StartupTask.GetAsync(StartupTaskname);
            return ConvertState(startupTask.State);
        }

        public async Task<StartupTaskStatus> EnableStartupTask()
        {
            var startupTask = await StartupTask.GetAsync(StartupTaskname);
            var newState = await startupTask.RequestEnableAsync();
            return ConvertState(newState);
        }

        public async Task DisableStartupTask()
        {
            var startupTask = await StartupTask.GetAsync(StartupTaskname);
            startupTask.Disable();
        }

        private StartupTaskStatus ConvertState(StartupTaskState state)
        {
            switch (state)
            {
                case StartupTaskState.Disabled:
                    return StartupTaskStatus.Disabled;
                case StartupTaskState.DisabledByUser:
                    return StartupTaskStatus.DisabledByUser;
                case StartupTaskState.Enabled:
                    return StartupTaskStatus.Enabled;
                case StartupTaskState.DisabledByPolicy:
                    return StartupTaskStatus.DisabledByPolicy;
            }
            throw new NotImplementedException();
        }
    }
}
