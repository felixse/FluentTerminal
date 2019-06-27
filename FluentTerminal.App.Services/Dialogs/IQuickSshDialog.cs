using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface IQuickSshDialog
    {
        Task<SshProfile> GetSshProfileAsync(SshProfile input = null);
    }
}