using FluentTerminal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Implementation
{
    public class FluentTerminalThemeParser : IThemeParser
    {
        public IEnumerable<string> SupportedFileTypes { get; } = new[] { ".flutecolors" };

        public Task<ExportedTerminalTheme> Import(string fileName, Stream fileContent)
        {
            return DeserializeThemeAsync<ExportedTerminalTheme>(fileName, fileContent);
        }

        public Task<TerminalTheme> Parse(string fileName, Stream fileContent)
        {
            return DeserializeThemeAsync<TerminalTheme>(fileName, fileContent);
        }

        private async Task<T> DeserializeThemeAsync<T>(string fileName, Stream fileContent)
            where T : TerminalTheme
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileContent == null)
            {
                throw new ArgumentNullException(nameof(fileContent));
            }

            using (var streamReader = new StreamReader(fileContent))
            {
                var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                var theme = JsonConvert.DeserializeObject<T>(content);

                theme.PreInstalled = false;
                theme.Id = Guid.NewGuid();

                return theme;
            }
        }
    }
}