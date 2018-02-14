using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public enum DialogButton
    {
        OK,
        Cancel
    }

    public interface IDialogService
    {
        Task<DialogButton> ShowDialogAsnyc(string title, string content, params DialogButton[] buttons);
    }
}
