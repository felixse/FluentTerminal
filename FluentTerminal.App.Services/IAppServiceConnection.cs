using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IAppServiceConnection
    {
        Task<IDictionary<string, string>> SendMessageAsync(IDictionary<string, string> message);
    }
}
