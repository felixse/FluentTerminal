using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface IInputDialog
    {
        void SetTitle(string title);
        Task<string> GetInput();
    }
}
