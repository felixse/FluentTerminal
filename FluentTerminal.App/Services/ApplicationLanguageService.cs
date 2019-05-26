using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;

namespace FluentTerminal.App.Services
{
    public class ApplicationLanguageService : IApplicationLanguageService
    {
        private readonly Dictionary<string, string> _languages = new Dictionary<string, string>
        {
            [""] = "System default",
            ["zh-Hans-CN"] = "Chinese (Traditional)",
            ["de"] = "Deutsch",
            ["en"] = "English",
            ["es"] = "Español"
        };

        public IEnumerable<string> Languages => _languages.Values;

        public void SetLanguage(string language)
        {
            if (!_languages.ContainsValue(language))
            {
                throw new ArgumentException($"Language not supported: {language}", nameof(language));
            }

            ApplicationLanguages.PrimaryLanguageOverride = _languages.FirstOrDefault(x => x.Value == language).Key;
        }

        public string GetCurrentLanguage()
        {
            return _languages[ApplicationLanguages.PrimaryLanguageOverride];
        }
    }
}
