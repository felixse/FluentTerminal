using System;

namespace FluentTerminal.App.Services
{
    public interface IUpdateService
    {
        void CheckForUpdate();
        Version GetCurrentVersion();
        Version GetLatestVersion();
    }
}
