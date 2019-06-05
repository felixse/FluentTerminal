using Windows.ApplicationModel.Resources;

namespace FluentTerminal.Models
{
    public static class Resources
    {
        public static string GetString(string resourceId) =>
            ResourceLoader.GetForCurrentView().GetString(resourceId.Replace('.', '/'));
    }
}