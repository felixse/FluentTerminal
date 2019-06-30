using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace FluentTerminal.App.Services
{
    public class ClipboardService : IClipboardService
    {
        /// <summary>
        /// Right trim whitespaces for each line.
        /// </summary>
        private static readonly Regex RTrimMultiLinesPattern = new Regex(@"([^\S\r\n]+)([\r\n])", RegexOptions.Compiled);

        private readonly ISettingsService _settingsService;

        public ClipboardService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public Task<string> GetText()
        {
            var content = Clipboard.GetContent();
            if (content.Contains(StandardDataFormats.Text))
            {
                return content.GetTextAsync().AsTask();
            }
            // Otherwise return a new task that just sends an empty string.
            return Task.FromResult(string.Empty);
        }

        public void SetText(string text)
        {
            if (_settingsService.GetApplicationSettings().RTrimCopiedLines)
            {
                text = RTrimMultiLinesPattern.Replace(text, "$2");
            }
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}