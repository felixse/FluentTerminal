using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IClipboardService
    {
        Task<string> GetTextAsync();

        void SetText(string text);
    }
}