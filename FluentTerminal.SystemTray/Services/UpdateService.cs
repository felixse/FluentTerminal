using Newtonsoft.Json;
using RestSharp;
using System;
using Windows.ApplicationModel;

namespace FluentTerminal.SystemTray.Services
{
	public class UpdateService
	{
		private const string apiEndpoint = "https://api.github.com";

		public bool GetLatestReleasVersion()
		{
			var restClient = new RestClient(apiEndpoint);
			var restRequest = new RestRequest("/repos/felixse/fluentterminal/releases", Method.GET);

			var restResponse = restClient.Execute(restRequest);
			if (restResponse.IsSuccessful)
			{
				dynamic restResponseData = JsonConvert.DeserializeObject(restResponse.Content);
				string tag = restResponseData[0].tag_name;
				var latestVersion = new Version(tag);
				var currentVersion = Package.Current.Id.Version;
				return latestVersion > new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision);
			}
			return false;
		}
	}
}