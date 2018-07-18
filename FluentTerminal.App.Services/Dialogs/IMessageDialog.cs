using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Dialogs
{
    public interface IMessageDialog
    {
        string Title { get; set; }
        string Content { get; set; }

        void AddButton(DialogButton button);

        Task<DialogButton> ShowAsync();
    }
}