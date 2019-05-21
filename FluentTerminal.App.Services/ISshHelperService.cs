using System;
using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ISshHelperService
    {
        bool IsSsh(Uri uri);

        ISshConnectionInfo ParseSsh(Uri uri);

        ShellProfile CreateShellProfile(ISshConnectionInfo sshConnectionInfo);

        Task<ShellProfile> GetSshShellProfileAsync();

        string ConvertToUri(ISshConnectionInfo sshConnectionInfo);
    }
}