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
        public const string DefaultSshProfileKey = "DefaultSshProfile";

        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IApplicationDataContainer _keyBindings;
        private readonly IApplicationDataContainer _localSettings;
        private readonly IApplicationDataContainer _roamingSettings;
        private readonly IApplicationDataContainer _shellProfiles;
        private readonly IApplicationDataContainer _sshProfiles;
        private readonly IApplicationDataContainer _themes;

        public SettingsService(IDefaultValueProvider defaultValueProvider, ApplicationDataContainers containers)
        {
            _defaultValueProvider = defaultValueProvider;
            _localSettings = containers.LocalSettings;
            _roamingSettings = containers.RoamingSettings;

            _themes = containers.Themes;
            _keyBindings = containers.KeyBindings;
            _shellProfiles = containers.ShellProfiles;
            _sshProfiles = containers.SshProfiles;


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

        public string ExportSettings()
        {
            var config = new
            {
                App = GetApplicationSettings(),
                KeyBindings = GetCommandKeyBindings(),
                TerminalOptions = GetTerminalOptions(),
                Themes = new List<TerminalTheme>(),
                Profiles = new List<ShellProfile>(),
                SshProfiles = new List<SshProfile>()
            };

            foreach (var theme in GetThemes().Where(x => !x.PreInstalled))
            {
                config.Themes.Add(theme);
            }

            foreach (var profile in GetShellProfiles().Where(x => !x.PreInstalled))
            {
                config.Profiles.Add(profile);
            }

            foreach (var profile in GetSshProfiles())
            {
                config.SshProfiles.Add(profile);
            }

            return JsonConvert.SerializeObject(config);
        }

        public void ImportSettings(string serializedSettings)
        {
            var config = new
            {
                App = GetApplicationSettings(),
                KeyBindings = new Dictionary<string, ICollection<KeyBinding>>(),
                Themes = new List<TerminalTheme>(),
                Profiles = new List<ShellProfile>(),
                SshProfiles = new List<SshProfile>(),
                TerminalOptions = GetTerminalOptions()
            };

            JsonConvert.PopulateObject(serializedSettings, config);

            SaveApplicationSettings(config.App);

            // Since we set each command sepaartely, we don't need all existing settings
            foreach (var pair in config.KeyBindings)
            {
                SaveKeyBindings(pair.Key, pair.Value);
            }

            // Can't create/modify pre-installed themes
            foreach (var theme in config.Themes.Where(x => !x.PreInstalled))
            {
                var existingTheme = GetTheme(theme.Id);
                if (existingTheme?.PreInstalled == true)
                {
                    continue;
                }
                SaveTheme(theme, existingTheme != null);
            }

            // Can't create pre-installed profiles
            foreach (var profile in config.Profiles.Where(x => !x.PreInstalled))
            {
                var existingProfile = GetShellProfile(profile.Id);
                var isNew = existingProfile.EqualTo(default(ShellProfile));

                // You can only edit certain parts of preinstalled profiles
                if (!isNew && existingProfile.PreInstalled)
                {
                    existingProfile.WorkingDirectory = profile.WorkingDirectory;
                    existingProfile.Arguments = profile.Arguments;
                    existingProfile.TabThemeId = profile.TabThemeId;
                    existingProfile.TerminalThemeId = profile.TerminalThemeId;
                    existingProfile.LineEndingTranslation = profile.LineEndingTranslation;
                    existingProfile.KeyBindings = profile.KeyBindings;
                    SaveShellProfile(profile, isNew);
                    continue;
                }
                SaveShellProfile(profile, isNew);
            }

            foreach (var profile in config.SshProfiles)
            {
                var existingProfile = GetSshProfile(profile.Id);
                var isNew = existingProfile.EqualTo(default(SshProfile));

                SaveSshProfile(profile, isNew);
            }

            SaveTerminalOptions(config.TerminalOptions);
        }

        public event EventHandler<ApplicationSettings> ApplicationSettingsChanged;

        public event EventHandler<TerminalTheme> ThemeAdded;
        public event EventHandler<Guid> CurrentThemeChanged;
        public event EventHandler<Guid> ThemeDeleted;

        public event EventHandler KeyBindingsChanged;

        public event EventHandler<ShellProfile> ShellProfileAdded;
        public event EventHandler<Guid> ShellProfileDeleted;

        public event EventHandler<SshProfile> SshProfileAdded;
        public event EventHandler<Guid> SshProfileDeleted;

        public event EventHandler<TerminalOptions> TerminalOptionsChanged;

        public void DeleteShellProfile(Guid id)
        {
            _shellProfiles.Delete(id.ToString());
            ShellProfileDeleted?.Invoke(this, id);
        }

        public void DeleteSshProfile(Guid id)
        {
            _sshProfiles.Delete(id.ToString());
            SshProfileDeleted?.Invoke(this, id);
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
        }


        public void DeleteTheme(Guid id)
        {
            _themes.Delete(id.ToString());

            foreach (var profile in GetShellProfiles())
            {
                if (profile.TerminalThemeId == id)
                {
                    profile.TerminalThemeId = Guid.Empty;
                    SaveShellProfile(profile);
                }
            }

            ThemeDeleted?.Invoke(this, id);
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
        public SshProfile GetDefaultSshProfile()
        {
            var id = GetDefaultSshProfileId();
            var profile = _sshProfiles.ReadValueFromJson(id.ToString(), default(SshProfile));

            return profile;
        }

        public ShellProfile GetShellProfile(Guid id)
        {
            return _shellProfiles.ReadValueFromJson(id.ToString(), default(ShellProfile));
        }
        public SshProfile GetSshProfile(Guid id)
        {
            return _sshProfiles.ReadValueFromJson(id.ToString(), default(SshProfile));
        }

        public Guid GetDefaultShellProfileId()
        {
            if (_localSettings.TryGetValue(DefaultShellProfileKey, out object value))
            {
                return (Guid)value;
            }
            return _defaultValueProvider.GetDefaultShellProfileId();
        }
        public Guid GetDefaultSshProfileId()
        {
            if (_localSettings.TryGetValue(DefaultSshProfileKey, out object value))
            {
                return (Guid)value;
            }
            return System.Guid.Empty;
        }

        public IDictionary<string, ICollection<KeyBinding>> GetCommandKeyBindings()
        {
            var keyBindings = new Dictionary<string, ICollection<KeyBinding>>();

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                keyBindings.Add(command.ToString(), _keyBindings.ReadValueFromJson<Collection<KeyBinding>>(command.ToString(), null) ?? _defaultValueProvider.GetDefaultKeyBindings(command));
            }
            return keyBindings;
        }

        public IEnumerable<ShellProfile> GetShellProfiles()
        {
            return _shellProfiles.GetAll().Select(x => JsonConvert.DeserializeObject<ShellProfile>((string)x)).ToList();
        }
        public IEnumerable<SshProfile> GetSshProfiles()
        {
            if (_sshProfiles == null)
                return new List<SshProfile>();
            else
                return _sshProfiles.GetAll()
                    .Select(x => JsonConvert.DeserializeObject<SshProfile>((string) x)).ToList();
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
        public void SaveDefaultSshProfileId(Guid id)
        {
            _localSettings.SetValue(DefaultSshProfileKey, id);
        }

        public void SaveKeyBindings(string command, ICollection<KeyBinding> keyBindings)
        {
            if (!Enum.TryParse<Command>(command, true, out var enumValue))
            {
                throw new InvalidOperationException();
            }

            _keyBindings.WriteValueAsJson(enumValue.ToString(), keyBindings);
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public void SaveShellProfile(ShellProfile shellProfile, bool newShell = false)
        {
            _shellProfiles.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile);

            // When saving the shell profile, we also need to update keybindings for everywhere.
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);

            if (newShell)
            {
                ShellProfileAdded?.Invoke(this, shellProfile);
            }
        }
        public void SaveSshProfile(SshProfile sshProfile, bool newShell = false)
        {
            _sshProfiles.WriteValueAsJson(sshProfile.Id.ToString(), sshProfile);

            // When saving the shell profile, we also need to update keybindings for everywhere.
            KeyBindingsChanged?.Invoke(this, System.EventArgs.Empty);

            if (newShell)
            {
                SshProfileAdded?.Invoke(this, sshProfile);
            }
        }

        public void SaveTerminalOptions(TerminalOptions terminalOptions)
        {
            _roamingSettings.WriteValueAsJson(nameof(TerminalOptions), terminalOptions);
            TerminalOptionsChanged?.Invoke(this, terminalOptions);
        }

        public void SaveTheme(TerminalTheme theme, bool newTheme = false)
        {
            _themes.WriteValueAsJson(theme.Id.ToString(), theme);

            if (theme.Id == GetCurrentThemeId())
            {
                CurrentThemeChanged?.Invoke(this, theme.Id);
            }

            if (newTheme)
            {
                ThemeAdded?.Invoke(this, theme);
            }
        }
    }
}