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
            ["ar"] = "العربية",
            ["ar-IQ"] = "اللهجة العراقية",
            ["az-Latn"] = "azərbaycan dili",
            ["bg"] = "български",
            ["bs"] = "bosanski",
            ["de"] = "Deutsch",
            ["en"] = "English",
            ["eo"] = "Esperanto",
            ["es"] = "Español",
            ["fa"] = "فارسی",
            ["fr"] = "Français",
            ["he"] = "עברית",
            ["hi"] = "हिन्दुस्तानी",
            ["hr"] = "Hrvatski",
            ["hu"] = "magyar",
            ["id"] = "Bahasa Indonesia",
            ["it"] = "Italiano",
            ["ja"] = "日本語",
            ["ko"] = "한국어",
            ["nl"] = "Nederlands",
            ["pl"] = "Polski",
            ["pt"] = "Português",
            ["pt-BR"] = "Português-Brasil",
            ["ro"] = "Română",
            ["ru"] = "Pусский",
            ["sl"] = "Slovenščina",
            ["sr-Latn"] = "srpski",
            ["sr-cyrl"] = "српски",
            ["sv"] = "svenska",
            ["sq"] = "shqip",
            ["tr"] = "Türkçe",
            ["ug-Arab"] = "ئۇيغۇر تىلى",
            ["uk"] = "Українська",
            ["uz-Latn"] = "O'zbek tili",
            ["vi"] = "tiếng việt",
            ["zh-Hans"] = "简体中文",
            ["zh-Hant"] = "繁體中文"
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
            if (!_languages.ContainsKey(ApplicationLanguages.PrimaryLanguageOverride))
            {
                ApplicationLanguages.PrimaryLanguageOverride = "en";
            }
            return _languages[ApplicationLanguages.PrimaryLanguageOverride];
        }
    }
}
