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
            return null;
        }

        public void SetText(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
