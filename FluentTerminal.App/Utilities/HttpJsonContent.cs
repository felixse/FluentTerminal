using Newtonsoft.Json;
using Windows.Web.Http;

namespace FluentTerminal.App.Utilities
{
    public static class HttpJsonContent
    {
        public static HttpStringContent From(object content)
        {
            var serialized = JsonConvert.SerializeObject(content);
            return new HttpStringContent(serialized, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
        }
    }
}
