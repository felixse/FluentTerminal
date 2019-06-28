﻿using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface ISshConnectionInfoDialog
    {
        Task<SshProfile> GetSshConnectionInfoAsync(SshProfile input = null);
    }
}