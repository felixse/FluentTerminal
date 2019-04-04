﻿using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface ISshConnectionInfoDialog
    {
        Task<ISshConnectionInfo> GetSshConnectionInfoAsync();
    }
}