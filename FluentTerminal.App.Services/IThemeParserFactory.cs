using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface IThemeParserFactory
    {
        IEnumerable<string> SupportedFileTypes { get; }

        IThemeParser GetParser(string fileType);
    }
}