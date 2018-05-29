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
        public const string ThemesContainerName = "Themes";
        public const string ShellProfilesContainerName = "ShellProfiles";
        public const string CurrentThemeKey = "CurrentTheme";
        public const string DefaultShellProfileKey = "DefaultShellProfile";

        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly ApplicationDataContainer _localSettings;
        private readonly ApplicationDataContainer _themes;
        private readonly ApplicationDataContainer _shellProfiles;
        private readonly ApplicationDataContainer _roamingSettings;

        public event EventHandler CurrentThemeChanged;
        public event EventHandler TerminalOptionsChanged;
        public event EventHandler ApplicationSettingsChanged;
        public event EventHandler KeyBindingsChanged;

        public SettingsService(IDefaultValueProvider defaultValueProvider)
        {
            _defaultValueProvider = defaultValueProvider;
            _localSettings = ApplicationData.Current.LocalSettings;
            _roamingSettings = ApplicationData.Current.RoamingSettings;

            _themes = _roamingSettings.CreateContainer(ThemesContainerName, ApplicationDataCreateDisposition.Always);
            _shellProfiles = _roamingSettings.CreateContainer(ShellProfilesContainerName, ApplicationDataCreateDisposition.Always);

            foreach (var theme in _defaultValueProvider.GetPreInstalledThemes())
            {
                _themes.WriteValueAsJson(theme.Id.ToString(), theme);
            }

            foreach (var shellProfile in _defaultValueProvider.GetPreinstalledShellProfiles())
            {
                _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);
            }
        }

        public TerminalTheme GetCurrentTheme()
        {
            var id = GetCurrentThemeId();
            return GetTheme(id);
        }

        public Guid GetCurrentThemeId()
        {
            if (_roamingSettings.Values.TryGetValue(CurrentThemeKey, out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultThemeId();
        }

        public void SaveCurrentThemeId(Guid id)
        {
            _roamingSettings.Values[CurrentThemeKey] = id;

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
            return _roamingSettings.ReadValueFromJson(nameof(TerminalOptions), _defaultValueProvider.GetDefaultTerminalOptions());
        }

        public void SaveTerminalOptions(TerminalOptions terminalOptions)
        {
            _roamingSettings.WriteValueAsJson(nameof(TerminalOptions), terminalOptions);
            TerminalOptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public ApplicationSettings GetApplicationSettings()
        {
            return _roamingSettings.ReadValueFromJson(nameof(ApplicationSettings), _defaultValueProvider.GetDefaultApplicationSettings());
        }

        public void SaveApplicationSettings(ApplicationSettings applicationSettings)
        {
            _roamingSettings.WriteValueAsJson(nameof(ApplicationSettings), applicationSettings);
            ApplicationSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public KeyBindings GetKeyBindings()
        {
            var defaults = _defaultValueProvider.GetDefaultKeyBindings();
            var keyBindings = _roamingSettings.ReadValueFromJson<KeyBindings>(nameof(KeyBindings), null) ?? defaults;

            if (keyBindings.ConfigurableNewTab == null)
            {
                keyBindings.ConfigurableNewTab = defaults.ConfigurableNewTab;
            }

            if (keyBindings.Search == null)
            {
                keyBindings.Search = defaults.Search;
            }

            return keyBindings;
        }

        public void SaveKeyBindings(KeyBindings keyBindings)
        {
            _roamingSettings.WriteValueAsJson(nameof(KeyBindings), keyBindings);
            KeyBindingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public Guid GetDefaultShellProfileId()
        {
            if (_roamingSettings.Values.TryGetValue(DefaultShellProfileKey, out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultShellProfileId();
        }

        public ShellProfile GetDefaultShellProfile()
        {
            var id = GetDefaultShellProfileId();
            return _shellProfiles.ReadValueFromJson(id.ToString(), default(ShellProfile));
        }

        public void SaveDefaultShellProfileId(Guid id)
        {
            _roamingSettings.Values[DefaultShellProfileKey] = id;
        }

        public IEnumerable<ShellProfile> GetShellProfiles()
        {
            return _shellProfiles.Values.Select(x => JsonConvert.DeserializeObject<ShellProfile>((string)x.Value)).ToList();
        }

        public void SaveShellProfile(ShellProfile shellProfile)
        {
            _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);
        }

        public void DeleteShellProfile(Guid id)
        {
            _shellProfiles.Values.Remove(id.ToString());
        }
    }
}