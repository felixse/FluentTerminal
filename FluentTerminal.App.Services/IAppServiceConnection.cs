using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IAppServiceConnection
    {
        event EventHandler<IDictionary<string, string>> MessageReceived;

        Task<IDictionary<string, string>> SendMessageAsync(IDictionary<string, string> message);
    }
}
