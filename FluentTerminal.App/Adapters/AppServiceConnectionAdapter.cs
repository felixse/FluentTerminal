using FluentTerminal.App.Services;
using System;
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

        public Task<ValueSet> SendMessageAsync(ValueSet message)
        {
            var response = await _appServiceConnection.SendMessageAsync(message).ConfigureAwait(false);

            if (response.Status == AppServiceResponseStatus.Success)
            {
                return response.Message;
            }
            return null;
        }
    }
}