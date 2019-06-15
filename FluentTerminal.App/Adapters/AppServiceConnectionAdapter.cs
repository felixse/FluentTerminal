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

        public event EventHandler<ValueSet> MessageReceived;

        public AppServiceConnectionAdapter(AppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
            _appServiceConnection.RequestReceived += OnRequestReceived;
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            MessageReceived?.Invoke(this, args.Request.Message);
        }

        public async Task<ValueSet> SendMessageAsync(ValueSet message)
        {
            var response = await _appServiceConnection.SendMessageAsync(message).AsTask();

            if (response.Status == AppServiceResponseStatus.Success)
            {
                return response.Message;
            }
            return null;
        }
    }
}