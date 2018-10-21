using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IUpdateService
    {
        void CheckForUpdate();
        Version GetCurrentVersion();
        Task<Version> GetLatestVersion();
    }
}
