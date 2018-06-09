using FluentTerminal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentTerminal.App.Services
{
    public interface IThemeParser
    {
        IEnumerable<string> SupportedFileTypes { get; }

        Task<TerminalTheme> Parse(StorageFile themeFile);
    }
}
