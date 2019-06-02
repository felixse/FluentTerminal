using System;
using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ISshHelperService
    {
        bool IsSsh(Uri uri);

        ISshConnectionInfo ParseSsh(Uri uri);

        Task<SshShellProfile> GetSshShellProfileAsync(SshShellProfile profile);

        Task<SshShellProfile> GetSavedSshShellProfileAsync();

        string ConvertToUri(ISshConnectionInfo sshConnectionInfo);
    }
}