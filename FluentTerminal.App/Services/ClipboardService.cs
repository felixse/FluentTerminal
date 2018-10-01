using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace FluentTerminal.App.Services
{
    public class ClipboardService : IClipboardService
    {
        public Task<string> GetText()
        {
            var content = Clipboard.GetContent();
            if (content.Contains(StandardDataFormats.Text))
            {
                return content.GetTextAsync().AsTask();
            }
            // Otherwise return a new task that just sends an empty string.
            return new Task<string>(new Func<string>(() => ""));
        }

        public void SetText(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}