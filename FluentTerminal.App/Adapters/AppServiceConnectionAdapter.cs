using FluentTerminal.App.Services;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FluentTerminal.App.Adapters
{
    public class AppServiceConnectionAdapter : IAppServiceConnection
    {
        private readonly AppServiceConnection _appServiceConnection;

        public event EventHandler<IDictionary<string, string>> MessageReceived;

        public AppServiceConnectionAdapter(AppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.RequestReceived += OnRequestReceived;
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageType = (string)args.Request.Message[MessageKeys.Type];
            var messageContent = (string)args.Request.Message[MessageKeys.Content];

            MessageReceived?.Invoke(this, new Dictionary<string, string>
            {
                [MessageKeys.Type] = messageType,
                [MessageKeys.Content] = messageContent
            });
        }

        public async Task<IDictionary<string, string>> SendMessageAsync(IDictionary<string, string> message)
        {
            var valueSet = new ValueSet();

            foreach (var pair in message)
            {
                valueSet.Add(pair.Key, pair.Value);
            }

            var response = await _appServiceConnection.SendMessageAsync(valueSet).AsTask();

            if (response.Status == AppServiceResponseStatus.Success)
            {
                var responseDict = new Dictionary<string, string>();
                foreach (var pair in response.Message)
                {
                    responseDict.Add(pair.Key, (string)pair.Value);
                }
                return responseDict;
            }
            return null;
        }
    }
}