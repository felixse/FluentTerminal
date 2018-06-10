using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentTerminal.Models;
using Newtonsoft.Json;
using Windows.Storage;

namespace FluentTerminal.App.Services.Implementation
{
    public class FluentTerminalThemeParser : IThemeParser
    {
        public IEnumerable<string> SupportedFileTypes { get; } = new string[] { ".flutecolors" };

        public async Task<TerminalTheme> Parse(StorageFile themeFile)
        {
            var content = await FileIO.ReadTextAsync(themeFile);

            var theme = JsonConvert.DeserializeObject<TerminalTheme>(content);

            theme.PreInstalled = false;
            theme.Id = Guid.NewGuid();

            return theme;
        }
    }
}
