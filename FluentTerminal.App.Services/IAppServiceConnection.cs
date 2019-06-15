using System;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace FluentTerminal.App.Services
{
    public interface IAppServiceConnection
    {
        event EventHandler<ValueSet> MessageReceived;

        Task<ValueSet> SendMessageAsync(ValueSet message);
    }
}