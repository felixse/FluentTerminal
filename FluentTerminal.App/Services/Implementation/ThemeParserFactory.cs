using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace FluentTerminal.App.Services.Implementation
{
    public class ThemeParserFactory : IThemeParserFactory
    {
        private IEnumerable<IThemeParser> _parsers;

        public IEnumerable<string> SupportedFileTypes { get; }

        public ThemeParserFactory(IEnumerable<IThemeParser> parsers)
        {
            _parsers = parsers;
            SupportedFileTypes = _parsers.SelectMany(p => p.SupportedFileTypes);
        }

        public IThemeParser GetParser(StorageFile themeFile)
        {
            return _parsers.FirstOrDefault(p => p.SupportedFileTypes.Contains(themeFile.FileType));
        }
    }
}
