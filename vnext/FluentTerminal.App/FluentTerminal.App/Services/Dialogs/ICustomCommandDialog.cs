using System.Threading.Tasks;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface ICustomCommandDialog
    {
        Task<ShellProfile> GetCustomCommandAsync(ShellProfile input = null);
    }
}