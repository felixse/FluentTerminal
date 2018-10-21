using System;

namespace FluentTerminal.App.Services
{
    public interface IUpdateService
    {
        void CheckForUpdate(bool notifyNoUpdate = false);
        Version GetCurrentVersion();
        Version GetLatestVersion();
    }
}
