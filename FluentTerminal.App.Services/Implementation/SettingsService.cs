using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;

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

        public event EventHandler<Guid> CurrentThemeChanged;
        public event EventHandler<TerminalOptions> TerminalOptionsChanged;
        public event EventHandler<ApplicationSettings> ApplicationSettingsChanged;
        public event EventHandler<Command?> KeyBindingsChanged;

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

        public TerminalTheme GetCurrentTheme()
        {
            var id = GetCurrentThemeId();
            return GetTheme(id);
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

            // Don't enumerate explicit keybinding enums that are in the range of a profile shortcut
            // since they won't be directly assigned to.
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                if (command < Command.ShellProfileShortcut)
                {
                    keyBindings.Add(command, _keyBindings.ReadValueFromJson<Collection<KeyBinding>>(command.ToString(), null) ?? _defaultValueProvider.GetDefaultKeyBindings(command));
                }
            }

            // Now, for each shell, find those with key bindings, and add them.
            foreach (ShellProfile shellProfile in GetShellProfiles())
            {
                if (shellProfile.KeyBinding != null)
                {
                    ICollection<KeyBinding> shellKeyBindings = shellProfile.KeyBinding;
                    // Use the command associated with the first key binding as representative for all of them.
                    keyBindings.Add(shellProfile.KeyBindingCommand, shellKeyBindings);
                }
            }

            return keyBindings;
        }

        public void SaveKeyBindings(Command command, ICollection<KeyBinding> keyBindings)
        {
            _keyBindings.WriteValueAsJson(command.ToString(), keyBindings);
            _roamingSettings.WriteValueAsJson(nameof(KeyBindings), keyBindings);
            KeyBindingsChanged?.Invoke(this, command);
        }

        public void ResetKeyBindings()
        {
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                // Don't enumerate explicit keybinding enums that are in the range of a profile shortcut
                // since they won't be directly assigned to.
                if (command >= Command.ShellProfileShortcut)
                {
                    continue;
                }

                _keyBindings.WriteValueAsJson(command.ToString(), _defaultValueProvider.GetDefaultKeyBindings(command));
            }

            KeyBindingsChanged?.Invoke(this, null);
        }

        public Guid GetDefaultShellProfileId()
        {
            if (_roamingSettings.TryGetValue(DefaultShellProfileKey, out object value))
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
            _roamingSettings.SetValue(DefaultShellProfileKey, id);
        }

        public IEnumerable<ShellProfile> GetShellProfiles()
        {
            return _shellProfiles.GetAll().Select(x => JsonConvert.DeserializeObject<ShellProfile>((string)x)).ToList();
        }

        public void SaveShellProfile(ShellProfile shellProfile, bool updateKeyBindings)
        {
            _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);

            if (updateKeyBindings)
            {
                KeyBindingsChanged?.Invoke(this, shellProfile.KeyBindingCommand);
            }
        }

        public void DeleteShellProfile(Guid id)
        {
            ShellProfile shellProfile = _shellProfiles.ReadValueFromJson<ShellProfile>(id.ToString(), null);
            _shellProfiles.Values.Remove(id.ToString());
            KeyBindingsChanged?.Invoke(this, shellProfile?.KeyBindingCommand);
            //_shellProfiles.Delete(id.ToString());
        }
    }
}