using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using Windows.Storage;

namespace FluentTerminal.App.Services.Implementation
{
    internal class SettingsService : ISettingsService
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private ApplicationDataContainer _localSettings;
        private ApplicationDataContainer _roamingSettings;

        public SettingsService(IDefaultValueProvider defaultValueProvider)
        {
            _defaultValueProvider = defaultValueProvider;
            _localSettings = ApplicationData.Current.LocalSettings;
            _roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationData.Current.DataChanged += OnDataChanged;
        }

        private void OnDataChanged(ApplicationData sender, object args)
        {
            //todo handle data changed for roaming settings
        }

        public ShellConfiguration GetShellConfiguration()
        {
            return _localSettings.ReadValueFromJson(nameof(ShellConfiguration), _defaultValueProvider.GetDefaultShellConfiguration());
        }

        public void SaveShellConfiguration(ShellConfiguration spawnConfiguration)
        {
            _localSettings.WriteValueAsJson(nameof(ShellConfiguration), spawnConfiguration);
        }

        
    }
}
