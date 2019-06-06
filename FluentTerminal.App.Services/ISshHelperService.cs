using System;
using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ISshHelperService
    {
        bool IsSsh(Uri uri);

        ISshConnectionInfo ParseSsh(Uri uri);

        Task<SshProfile> GetSshProfileAsync(SshProfile profile);

        Task<SshProfile> GetSavedSshProfileAsync();

        string ConvertToUri(ISshConnectionInfo sshConnectionInfo);
    }
}