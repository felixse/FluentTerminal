using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FluentTerminal.App.Services.Implementation
{
    public class SettingsService : ISettingsService
    {
        public const string CurrentThemeKey = "CurrentTheme";
        public const string DefaultShellProfileKey = "DefaultShellProfile";

        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IApplicationDataContainer _keyBindings;
        private readonly IApplicationDataContainer _localSettings;
        private readonly IApplicationDataContainer _roamingSettings;
        private readonly IApplicationDataContainer _shellProfiles;
        private readonly IApplicationDataContainer _themes;
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
                if (GetTheme(theme.Id) == null)
                {
                    _themes.WriteValueAsJson(theme.Id.ToString(), theme);
                }
            }

            foreach (var shellProfile in _defaultValueProvider.GetPreinstalledShellProfiles())
            {
                if (GetShellProfile(shellProfile.Id) == null)
                {
                    _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);
                }
            }
        }

        public event EventHandler<ApplicationSettings> ApplicationSettingsChanged;

        public event EventHandler<Guid> CurrentThemeChanged;

        public event EventHandler KeyBindingsChanged;

        /// <summary>
        /// Indicate that a change has occurred to the shell profile collection. If a profile was added, the furst tuple value will be true, and if a profile was removed it will be false.
        /// </summary>
        public event EventHandler<Tuple<bool, ShellProfile>> ShellProfileCollectionChanged;

        public event EventHandler<TerminalOptions> TerminalOptionsChanged;
        public void DeleteShellProfile(Guid id)
        {
            _shellProfiles.Delete(id.ToString());
            ShellProfileCollectionChanged?.Invoke(this, new Tuple<bool, ShellProfile>(false, GetShellProfile(id)));
        }

        public void DeleteTheme(Guid id)
        {
            _themes.Delete(id.ToString());
        }

        public ApplicationSettings GetApplicationSettings()
        {
            return _roamingSettings.ReadValueFromJson(nameof(ApplicationSettings), _defaultValueProvider.GetDefaultApplicationSettings());
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

        public ShellProfile GetShellProfile(Guid id)
        {
            return _shellProfiles.ReadValueFromJson(id.ToString(), default(ShellProfile));
        }

        public Guid GetDefaultShellProfileId()
        {
            if (_localSettings.TryGetValue(DefaultShellProfileKey, out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultShellProfileId();
        }

        /// <summary>
        /// Attempt to parse a command string, given what the SettingsService knows about available application commands, as well as other dynamic runtime commands.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>ICommand instance that describes the command.</returns>
        public AbstractCommand ParseCommandString(string command)
        {
            Command e;
            if (Enum.TryParse(command, true, out e))
            {
                return e;
            }
            else
            {
                foreach (ShellProfile profile in GetShellProfiles())
                {
                    AbstractCommand acmd = profile;
                    if (command == acmd.ToString())
                    {
                        return acmd;
                    }
                }
                return null;
            }
        }

        public IDictionary<AbstractCommand, ICollection<KeyBinding>> GetKeyBindings()
        {
            var keyBindings = new Dictionary<AbstractCommand, ICollection<KeyBinding>>();

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                keyBindings.Add(command, _keyBindings.ReadValueFromJson<Collection<KeyBinding>>(command.ToString(), null) ?? _defaultValueProvider.GetDefaultKeyBindings(command));
            }

            foreach (ShellProfile profile in GetShellProfiles())
            {
                // For each shell, list all of the key bindings, and add them to the KeyBindings mapping.
                if (profile.KeyBindings != null)
                {
                    keyBindings.Add(profile, profile.KeyBindings);
                }
            }

            return keyBindings;
        }

        public IEnumerable<ShellProfile> GetShellProfiles()
        {
            return _shellProfiles.GetAll().Select(x => JsonConvert.DeserializeObject<ShellProfile>((string)x)).ToList();
        }

        public IEnumerable<TabTheme> GetTabThemes()
        {
            return _defaultValueProvider.GetDefaultTabThemes();
        }

        public TerminalOptions GetTerminalOptions()
        {
            return _roamingSettings.ReadValueFromJson(nameof(TerminalOptions), _defaultValueProvider.GetDefaultTerminalOptions());
        }

        public TerminalTheme GetTheme(Guid id)
        {
            return _themes.ReadValueFromJson(id.ToString(), default(TerminalTheme));
        }

        public IEnumerable<TerminalTheme> GetThemes()
        {
            return _themes.GetAll().Select(x => JsonConvert.DeserializeObject<TerminalTheme>((string)x)).ToList();
        }

        public void ResetKeyBindings()
        {
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                _keyBindings.WriteValueAsJson(command.ToString(), _defaultValueProvider.GetDefaultKeyBindings(command));
            }

            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public void SaveApplicationSettings(ApplicationSettings applicationSettings)
        {
            _roamingSettings.WriteValueAsJson(nameof(ApplicationSettings), applicationSettings);
            ApplicationSettingsChanged?.Invoke(this, applicationSettings);
        }

        public void SaveCurrentThemeId(Guid id)
        {
            _roamingSettings.SetValue(CurrentThemeKey, id);

            CurrentThemeChanged?.Invoke(this, id);
        }

        public void SaveDefaultShellProfileId(Guid id)
        {
            _localSettings.SetValue(DefaultShellProfileKey, id);
        }

        public void SaveKeyBindings(AbstractCommand command, ICollection<KeyBinding> keyBindings)
        {
            _keyBindings.WriteValueAsJson(command.ToString(), keyBindings);
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public void SaveShellProfile(ShellProfile shellProfile, bool newShell = false)
        {
            _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);
            // When saving the shell profile, we also need to update keybindings for everywhere.
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
            if (newShell)
            {
                ShellProfileCollectionChanged?.Invoke(this, new Tuple<bool, ShellProfile>(true, shellProfile));
            }
        }

        public void SaveTerminalOptions(TerminalOptions terminalOptions)
        {
            _roamingSettings.WriteValueAsJson(nameof(TerminalOptions), terminalOptions);
            TerminalOptionsChanged?.Invoke(this, terminalOptions);
        }

        public void SaveTheme(TerminalTheme theme)
        {
            _themes.WriteValueAsJson(theme.Id.ToString(), theme);

            if (theme.Id == GetCurrentThemeId())
            {
                CurrentThemeChanged?.Invoke(this, theme.Id);
            }
        }
    }
}