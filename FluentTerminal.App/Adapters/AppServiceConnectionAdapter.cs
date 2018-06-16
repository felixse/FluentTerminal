using FluentTerminal.App.Services;
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

        public AppServiceConnectionAdapter(AppServiceConnection appServiceConnection)
        {
            _appServiceConnection = appServiceConnection;
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
