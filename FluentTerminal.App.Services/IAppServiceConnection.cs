using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IAppServiceConnection
    {
        event EventHandler<SerializedMessage> MessageReceived;

        Task<SerializedMessage> SendMessageAsync(SerializedMessage message);
    }
}