using System;
using Windows.ApplicationModel;
using Newtonsoft.Json;
using RestSharp;

namespace FluentTerminal.App.Services.Implementation
{
    public class UpdateService2 : IUpdateService
    {
        private const string apiEndpoint = "https://api.github.com";

        public void CheckForUpdate()
        {
            if (GetLatestVersion() > GetCurrentVersion())
            {

            }
        }

        public Version GetCurrentVersion()
        {
            var currentVersion = Package.Current.Id.Version;
            return new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision);
        }

        public Version GetLatestVersion()
        {
            var restClient = new RestClient(apiEndpoint);
            var restRequest = new RestRequest("/repos/felixse/fluentterminal/releases", Method.GET);

            var restResponse = restClient.Execute(restRequest);
            if (restResponse.IsSuccessful)
            {
                dynamic restResponseData = JsonConvert.DeserializeObject(restResponse.Content);
                string tag = restResponseData[0].tag_name;
                var latestVersion = new Version(tag);
                return new Version(latestVersion.Major, latestVersion.Minor, latestVersion.Build, latestVersion.Revision);
            }
            return new Version(3, 14, 0, 0);
        }
    }
}
