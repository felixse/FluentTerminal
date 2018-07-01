using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentTerminal.Models;
using Newtonsoft.Json;

namespace FluentTerminal.App.Services.Implementation
{
    public class FluentTerminalThemeParser : IThemeParser
    {
        public IEnumerable<string> SupportedFileTypes { get; } = new string[] { ".flutecolors" };

        public async Task<TerminalTheme> Parse(string fileName, Stream fileContent)
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
                var content = await streamReader.ReadToEndAsync();
                var theme = JsonConvert.DeserializeObject<TerminalTheme>(content);

                theme.PreInstalled = false;
                theme.Id = Guid.NewGuid();

                return theme;
            }
        }
    }
}
