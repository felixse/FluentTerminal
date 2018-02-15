using System;
using System.Collections.Generic;
using System.Linq;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using Newtonsoft.Json;
using Windows.Storage;

namespace FluentTerminal.App.Services.Implementation
{
    internal class SettingsService : ISettingsService
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private ApplicationDataContainer _localSettings;
        private ApplicationDataContainer _themes;
        private ApplicationDataContainer _roamingSettings;

        public event EventHandler CurrentThemeChanged;
        public event EventHandler TerminalOptionsChanged;

        public SettingsService(IDefaultValueProvider defaultValueProvider)
        {
            _defaultValueProvider = defaultValueProvider;
            _localSettings = ApplicationData.Current.LocalSettings;
            _roamingSettings = ApplicationData.Current.RoamingSettings;

            _themes = _roamingSettings.CreateContainer("Themes", ApplicationDataCreateDisposition.Always);

            foreach (var theme in _defaultValueProvider.GetPreInstalledThemes())
            {
                _themes.WriteValueAsJson(theme.Id.ToString(), theme);
            }

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

        public void SaveShellConfiguration(ShellConfiguration shellConfiguration)
        {
            _localSettings.WriteValueAsJson(nameof(ShellConfiguration), shellConfiguration);
        }

        public TerminalTheme GetCurrentTheme()
        {
            var id = GetCurrentThemeId();
            return GetTheme(id);
        }

        public Guid GetCurrentThemeId()
        {
            if (_roamingSettings.Values.TryGetValue("CurrentTheme", out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultThemeId();
        }

        public void SaveCurrentThemeId(Guid id)
        {
            _roamingSettings.Values["CurrentTheme"] = id;

            CurrentThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveTheme(TerminalTheme theme)
        {
            _themes.WriteValueAsJson(theme.Id.ToString(), theme);

            if (theme.Id == GetCurrentThemeId())
            {
                CurrentThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void DeleteTheme(Guid id)
        {
            _themes.Values.Remove(id.ToString());
        }

        public IEnumerable<TerminalTheme> GetThemes()
        {
            return _themes.Values.Select(x => JsonConvert.DeserializeObject<TerminalTheme>((string)x.Value)).ToList();
        }

        public TerminalTheme GetTheme(Guid id)
        {
            return _themes.ReadValueFromJson(id.ToString(), default(TerminalTheme));
        }

        public TerminalOptions GetTerminalOptions()
        {
            return _localSettings.ReadValueFromJson(nameof(TerminalOptions), _defaultValueProvider.GetDefaultTerminalOptions());
        }

        public void SaveTerminalOptions(TerminalOptions terminalOptions)
        {
            _localSettings.WriteValueAsJson(nameof(TerminalOptions), terminalOptions);
            TerminalOptionsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}