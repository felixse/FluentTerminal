using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace FluentTerminal.App.Services.Implementation
{
    public class SettingsService : ISettingsService
    {
        public const string CurrentThemeKey = "CurrentTheme";
        public const string DefaultShellProfileKey = "DefaultShellProfile";

        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IApplicationDataContainer _localSettings;
        private readonly IApplicationDataContainer _themes;
        private readonly IApplicationDataContainer _keyBindings;
        private readonly IApplicationDataContainer _shellProfiles;
        private readonly IApplicationDataContainer _roamingSettings;
        private Dictionary<string, Tuple<byte, byte, byte, byte, double>> windowsThemeColours;

        public event EventHandler<Guid> CurrentThemeChanged;

        public event EventHandler<TerminalOptions> TerminalOptionsChanged;

        public event EventHandler<ApplicationSettings> ApplicationSettingsChanged;

        public event EventHandler KeyBindingsChanged;

        public SettingsService(IDefaultValueProvider defaultValueProvider, ApplicationDataContainers containers)
        {
            _defaultValueProvider = defaultValueProvider;
            _localSettings = containers.LocalSettings;
            _roamingSettings = containers.RoamingSettings;

            _themes = containers.Themes;
            _keyBindings = containers.KeyBindings;
            _shellProfiles = containers.ShellProfiles;

            foreach (var theme in _defaultValueProvider.GetPreInstalledThemes())
            {
                _themes.WriteValueAsJson(theme.Id.ToString(), theme);
            }

            foreach (var shellProfile in _defaultValueProvider.GetPreinstalledShellProfiles())
            {
                _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);
            }
        }

        public string GetWindowsThemeColorString(string colorKey, bool mixOpacity)
        {
            // I would prefer to use the App.Utilities.ColorExtensions methods, but the nature of the
            // references means that, given the current file layout, i can't.
            Tuple<byte, byte, byte, byte, double> t = windowsThemeColours[colorKey];
            byte r = t.Item1;
            byte g = t.Item2;
            byte b = t.Item3;
            byte a = t.Item4;
            double opacity = t.Item5;

            string ret = $"#{a:X2}{r:X2}{g:X2}{b:X2}";

            return ret;
        }

        public void SetWindowsThemeColours(Dictionary<string, Tuple<byte, byte, byte, byte, double>> themeColors)
        {
            windowsThemeColours = themeColors;
        }

        public TerminalTheme GetCurrentTheme()
        {
            var id = GetCurrentThemeId();
            var theme = GetTheme(id);
            if (theme == null)
            {
                id = _defaultValueProvider.GetDefaultThemeId();
                SaveCurrentThemeId(id);
                theme = GetTheme(id);
            }
            return theme;
        }

        public Guid GetCurrentThemeId()
        {
            if (_roamingSettings.TryGetValue(CurrentThemeKey, out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultThemeId();
        }

        public void SaveCurrentThemeId(Guid id)
        {
            _roamingSettings.SetValue(CurrentThemeKey, id);

            CurrentThemeChanged?.Invoke(this, id);
        }

        public void SaveTheme(TerminalTheme theme)
        {
            _themes.WriteValueAsJson(theme.Id.ToString(), theme);

            if (theme.Id == GetCurrentThemeId())
            {
                CurrentThemeChanged?.Invoke(this, theme.Id);
            }
        }

        public void DeleteTheme(Guid id)
        {
            _themes.Delete(id.ToString());
        }

        public IEnumerable<TerminalTheme> GetThemes()
        {
            return _themes.GetAll().Select(x => JsonConvert.DeserializeObject<TerminalTheme>((string)x)).ToList();
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
            TerminalOptionsChanged?.Invoke(this, terminalOptions);
        }

        public ApplicationSettings GetApplicationSettings()
        {
            return _roamingSettings.ReadValueFromJson(nameof(ApplicationSettings), _defaultValueProvider.GetDefaultApplicationSettings());
        }

        public void SaveApplicationSettings(ApplicationSettings applicationSettings)
        {
            _roamingSettings.WriteValueAsJson(nameof(ApplicationSettings), applicationSettings);
            ApplicationSettingsChanged?.Invoke(this, applicationSettings);
        }

        public IDictionary<Command, ICollection<KeyBinding>> GetKeyBindings()
        {
            var keyBindings = new Dictionary<Command, ICollection<KeyBinding>>();
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                keyBindings.Add(command, _keyBindings.ReadValueFromJson<Collection<KeyBinding>>(command.ToString(), null) ?? _defaultValueProvider.GetDefaultKeyBindings(command));
            }

            return keyBindings;
        }

        public void SaveKeyBindings(Command command, ICollection<KeyBinding> keyBindings)
        {
            _keyBindings.WriteValueAsJson(command.ToString(), keyBindings);
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public void ResetKeyBindings()
        {
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                _keyBindings.WriteValueAsJson(command.ToString(), _defaultValueProvider.GetDefaultKeyBindings(command));
            }

            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public Guid GetDefaultShellProfileId()
        {
            if (_localSettings.TryGetValue(DefaultShellProfileKey, out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultShellProfileId();
        }

        public ShellProfile GetDefaultShellProfile()
        {
            var id = GetDefaultShellProfileId();
            var profile = _shellProfiles.ReadValueFromJson(id.ToString(), default(ShellProfile));
            if (profile == null)
            {
                id = _defaultValueProvider.GetDefaultShellProfileId();
                SaveDefaultShellProfileId(id);
                profile = _shellProfiles.ReadValueFromJson(id.ToString(), default(ShellProfile));
            }
            return profile;
        }

        public void SaveDefaultShellProfileId(Guid id)
        {
            _localSettings.SetValue(DefaultShellProfileKey, id);
        }

        public IEnumerable<ShellProfile> GetShellProfiles()
        {
            return _shellProfiles.GetAll().Select(x => JsonConvert.DeserializeObject<ShellProfile>((string)x)).ToList();
        }

        public void SaveShellProfile(ShellProfile shellProfile)
        {
            _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);
        }

        public void DeleteShellProfile(Guid id)
        {
            _shellProfiles.Delete(id.ToString());
        }
    }
}