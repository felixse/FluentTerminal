using System.Collections.Generic;
using Windows.Storage;

namespace FluentTerminal.App.Services
{
    public interface IThemeParserFactory
    {
        IEnumerable<string> SupportedFileTypes { get; }

        IThemeParser GetParser(StorageFile themeFile);
    }
}
