using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IUpdateService
    {
        Task CheckForUpdate(bool notifyNoUpdate = false);
        Version GetCurrentVersion();
        Task<Version> GetLatestVersionAsync();
    }
}
