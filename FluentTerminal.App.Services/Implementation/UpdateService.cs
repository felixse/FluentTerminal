using System;
using Windows.ApplicationModel;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services.Implementation
{
    public class UpdateService : IUpdateService
    {
        private const string apiEndpoint = "https://api.github.com";

        private readonly INotificationService _notificationService;

        public UpdateService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task CheckForUpdate(bool notifyNoUpdate = false)
        {
            var latest = await GetLatestVersionAsync();
            if (latest > GetCurrentVersion())
            {
                _notificationService.ShowNotification("Update available",
                    "Click to open the releases page.", "https://github.com/jumptrading/FluentTerminal/releases");
            }
            else if (notifyNoUpdate)
            {
                _notificationService.ShowNotification("No update available", "You're up to date!");
            }
        }

        public Version GetCurrentVersion()
        {
            var currentVersion = Package.Current.Id.Version;
            return new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision);
        }

        public async Task<Version> GetLatestVersionAsync()
        {
            var restClient = new RestClient(apiEndpoint);
            var restRequest = new RestRequest("/repos/jumptrading/fluentterminal/releases", Method.GET);

            var restResponse = await restClient.ExecuteTaskAsync(restRequest);
            if (restResponse.IsSuccessful)
            {
                dynamic restResponseData = JsonConvert.DeserializeObject(restResponse.Content);
                string tag = restResponseData[0].tag_name;
                if (tag.Split('-').Length == 3)
                {
                    var latestVersion = new Version(tag.Split('-')[1]);
                    return new Version(latestVersion.Major, latestVersion.Minor, latestVersion.Build, latestVersion.Revision);
                }
            }
            return new Version(0, 0, 0, 0);
        }
    }
}
