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
            ["ar-IQ"] = "اللهجة العراقية",
            ["az"] = "azərbaycan dili",
            ["de"] = "Deutsch",
            ["en"] = "English",
            ["es"] = "Español",
            ["fr"] = "Français",
            ["he"] = "עברית",
            ["hi"] = "हिन्दुस्तानी",
            ["it"] = "Italiano",
            ["ja"] = "日本語",
            ["ko"] = "한국어",
            ["nl"] = "Nederlands",
            ["pl"] = "Polski",
            ["pt-BR"] = "Português-Brasil",
            ["ro"] = "Română",
            ["ru"] = "Pусский",
            ["uk"] = "Українська",
            ["zh-Hans-CN"] = "Chinese (Traditional)"
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
