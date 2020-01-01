using Windows.ApplicationModel.Resources;

namespace FluentTerminal.App.Services.Utilities
{
    /// <summary>
    /// Get strings in different languages.
    /// Author: Zuozishi
    /// </summary>
    public static class I18N
    {
        private static readonly ResourceLoader ResourceLoader = ResourceLoader.GetForViewIndependentUse();

        public static string Translate(string resource)
        {
            return ResourceLoader.GetString(resource.Replace('.', '/'));
        }

        public static string TranslateWithFallback(string resource, string fallback)
        {
            var result = Translate(resource);

            return string.IsNullOrEmpty(result) ? fallback : result;
        }
    }
}
