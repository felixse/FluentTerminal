using FluentTerminal.Models;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface ICreateKeyBindingDialog
    {
        Task<KeyBinding> CreateKeyBinding();
    }
}