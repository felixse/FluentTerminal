using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace FluentTerminal.App.Utilities
{
    public static class IHttpContentExtensions
    {
        public static async Task<T> ReadAs<T>(this IHttpContent content)
        {
            var contentBody = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(contentBody);
        }
    }
}
