using FluentTerminal.Models.Enums;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IStartupTaskService
    {
        Task DisableStartupTaskAsync();
        Task<StartupTaskStatus> EnableStartupTaskAsync();
        Task<StartupTaskStatus> GetStatusAsync();
    }
}
