using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace FluentTerminal.App.Services.Utilities
{
    /// <summary>
    /// Get strings in different languages.
    /// Author: Zuozishi
    /// </summary>
    public static class StringsHelper
    {
        public static string GetString(string resource)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            return resourceLoader.GetString(resource);
        }
    }
}
