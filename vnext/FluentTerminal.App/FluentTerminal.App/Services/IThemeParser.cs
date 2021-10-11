using FluentTerminal.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IThemeParser
    {
        IEnumerable<string> SupportedFileTypes { get; }

        Task<TerminalTheme> Parse(string fileName, Stream fileContent);

        Task<ExportedTerminalTheme> Import(string fileName, Stream fileContent);
    }
}