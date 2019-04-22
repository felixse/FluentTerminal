using System;
using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ISshHelperService
    {
        bool IsSsh(Uri uri);

        Task<ShellProfile> GetSshShellProfileAsync();

        Task<ShellProfile> GetSshShellProfileAsync(Uri uri);

        string ConvertToUri(ISshConnectionInfo sshConnectionInfo);
    }
}