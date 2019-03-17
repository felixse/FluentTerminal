using FluentTerminal.App.Services;
using FluentTerminal.Models;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FluentTerminal.App.Adapters
{
    public class AppServiceConnectionAdapter : IAppServiceConnection
    {
        private readonly AppServiceConnection _appServiceConnection;

        public event EventHandler<SerializedMessage> MessageReceived;

        public AppServiceConnectionAdapter(AppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.RequestReceived += OnRequestReceived;
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageType = (byte)args.Request.Message[MessageKeys.Type];
            var messageContent = (byte[])args.Request.Message[MessageKeys.Content];

            MessageReceived?.Invoke(this, new SerializedMessage(messageType, messageContent));
        }

        public async Task<SerializedMessage> SendMessageAsync(SerializedMessage message)
        {
            var valueSet = new ValueSet
            {
                [MessageKeys.Type] = message.Identifier,
                [MessageKeys.Content] = message.Data
            };

            var response = await _appServiceConnection.SendMessageAsync(valueSet).AsTask();

            if (response.Status == AppServiceResponseStatus.Success && response.Message.Count > 0)
            {
                var messageType = (byte)response.Message[MessageKeys.Type];
                var messageContent = (byte[])response.Message[MessageKeys.Content];

                return new SerializedMessage(messageType, messageContent);
            }
            return null;
        }
    }
}