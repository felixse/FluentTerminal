﻿using Windows.ApplicationModel.Resources;

namespace FluentTerminal.App.Services.Utilities
{
    /// <summary>
    /// Get strings in different languages.
    /// Author: Zuozishi
    /// </summary>
    public static class I18N
    {
        public static string Translate(string resource)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            return resourceLoader.GetString(resource.Replace('.', '/'));
        }

        public static string TranslateWithFallback(string resource, string fallback)
        {
            var result = Translate(resource);

            return string.IsNullOrEmpty(result) ? fallback : result;
        }
    }
}
