using FluentTerminal.Models.Enums;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IStartupTaskService
    {
        Task DisableStartupTask();
        Task<StartupTaskStatus> EnableStartupTask();
        Task<StartupTaskStatus> GetStatus();
    }
}
