using System.Collections.Generic;
using System.Linq;

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

        public IThemeParser GetParser(string fileType)
        {
            return _parsers.FirstOrDefault(p => p.SupportedFileTypes.Contains(fileType));
        }
    }
}