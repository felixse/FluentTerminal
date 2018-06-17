using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IClipboardService
    {
        Task<string> GetText();
        void SetText(string text);
    }
}
