using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentTerminal.Models.Messages;
using GalaSoft.MvvmLight.Messaging;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class SettingsServiceTests
    {
        private readonly Fixture _fixture;

        public SettingsServiceTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Constructor_Default_WritesAllPreinstalledThemesInThemeContainer()
        {
            var themes = _fixture.CreateMany<TerminalTheme>(3);
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            defaultValueProvider.Setup(x => x.GetPreInstalledThemes()).Returns(themes);
            var themesContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };

            new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            defaultValueProvider.Verify(x => x.GetPreInstalledThemes(), Times.Once);
            foreach (var theme in themes)
            {
                themesContainer.Verify(x => x.WriteValueAsJson(theme.Id.ToString(), theme), Times.Once);
            }
        }

        [Fact]
        public void Constructor_Default_WritesAllPreinstalledShellProfilesInShellProfilesContainer()
        {
            var shellProfiles = _fixture.CreateMany<ShellProfile>(3);
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            defaultValueProvider.Setup(x => x.GetPreinstalledShellProfiles()).Returns(shellProfiles);
            var shellProfilesContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                ShellProfiles = shellProfilesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };

            new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            defaultValueProvider.Verify(x => x.GetPreinstalledShellProfiles(), Times.Once);
            foreach (var shellProfile in shellProfiles)
            {
                shellProfilesContainer.Verify(x => x.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile), Times.Once);
            }
        }

        [Fact]
        public void GetCurrentThemeId_ValueExistsInRoamingSettings_ReturnsCurrentThemeId()
        {
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var currentThemeId = (object)_fixture.Create<Guid>();
            roamingSettings.Setup(x => x.TryGetValue(SettingsService.CurrentThemeKey, out currentThemeId)).Returns(true);
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetCurrentThemeId();

            result.Should().Be((Guid)currentThemeId);
            roamingSettings.Verify(x => x.TryGetValue(SettingsService.CurrentThemeKey, out currentThemeId), Times.Once);
        }

        [Fact]
        public void GetCurrentThemeId_ValueDoesNotExistInRoamingSettings_ReturnsDefaultThemeIdFromDefaultValueProvider()
        {
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            var defaultThemeId = _fixture.Create<Guid>();
            defaultValueProvider.Setup(x => x.GetDefaultThemeId()).Returns(defaultThemeId);
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var currentThemeId = (object)_fixture.Create<Guid>();
            roamingSettings.Setup(x => x.TryGetValue(SettingsService.CurrentThemeKey, out currentThemeId)).Returns(false);
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            var result = settingsService.GetCurrentThemeId();

            result.Should().Be(defaultThemeId);
            defaultValueProvider.Verify(x => x.GetDefaultThemeId(), Times.Once);
            roamingSettings.Verify(x => x.TryGetValue(SettingsService.CurrentThemeKey, out currentThemeId), Times.Once);
        }

        [Fact]
        public void SaveCurrentThemeId_Default_SetsValueOnRoamingSettings()
        {
            var currentThemeId = _fixture.Create<Guid>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.SaveCurrentThemeId(currentThemeId);

            roamingSettings.Verify(x => x.SetValue(SettingsService.CurrentThemeKey, currentThemeId), Times.Once);
        }

        [Fact]
        public void SaveCurrentThemeId_Default_InvokesCurrentThemeChangedEvent()
        {
            var currentThemeId = _fixture.Create<Guid>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var currentThemeChangedEventInvoked = false;
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);
            Messenger.Default.Register<CurrentThemeChangedMessage>(this,
                message => currentThemeChangedEventInvoked = true);

            settingsService.SaveCurrentThemeId(currentThemeId);

            currentThemeChangedEventInvoked.Should().BeTrue();
        }

        [Fact]
        public void SaveTheme_Default_WritesToThemesContainer()
        {
            var theme = _fixture.Create<TerminalTheme>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var themesContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.SaveTheme(theme);

            themesContainer.Verify(x => x.WriteValueAsJson(theme.Id.ToString(), theme), Times.Once);
        }

        [Fact]
        public void SaveTheme_ThemeIsCurrentTheme_InvokesCurrentThemeChangedEvent()
        {
            var theme = _fixture.Create<TerminalTheme>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var themesContainer = new Mock<IApplicationDataContainer>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var currentThemeId = (object)theme.Id;
            roamingSettings.Setup(x => x.TryGetValue(SettingsService.CurrentThemeKey, out currentThemeId)).Returns(true);
            var currentThemeChangedEventInvoked = false;
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = roamingSettings.Object,
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);
            Messenger.Default.Register<CurrentThemeChangedMessage>(this,
                message => currentThemeChangedEventInvoked = true);

            settingsService.SaveTheme(theme);

            currentThemeChangedEventInvoked.Should().BeTrue();
        }

        [Fact]
        public void DeleteTheme_Default_CallsDeleteOnThemesContainer()
        {
            var themeId = _fixture.Create<Guid>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var themesContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.DeleteTheme(themeId);

            themesContainer.Verify(x => x.Delete(themeId.ToString()), Times.Once);
        }

        [Fact]
        public void GetThemes_Default_CallsGetAllOnThemesContainer()
        {
            var themes = _fixture.CreateMany<TerminalTheme>(3);
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var themesContainer = new Mock<IApplicationDataContainer>();
            themesContainer.Setup(x => x.GetAll()).Returns(themes.Select(JsonConvert.SerializeObject));
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetThemes();

            result.Should().BeEquivalentTo(themes);
            themesContainer.Verify(x => x.GetAll(), Times.Once);
        }

        [Fact]
        public void GetTheme_ValidThemeId_ReturnsThemeFromThemesContainer()
        {
            var theme = _fixture.Create<TerminalTheme>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var themesContainer = new Mock<IApplicationDataContainer>();
            themesContainer.Setup(x => x.ReadValueFromJson(theme.Id.ToString(), default(TerminalTheme))).Returns(theme);
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetTheme(theme.Id);

            result.Should().BeEquivalentTo(theme);
            themesContainer.Verify(x => x.ReadValueFromJson(theme.Id.ToString(), default(TerminalTheme)), Times.Once);
        }

        [Fact]
        public void GetTheme_UnknownThemeId_ReturnsNull()
        {
            var themeId = _fixture.Create<Guid>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var themesContainer = new Mock<IApplicationDataContainer>();
            themesContainer.Setup(x => x.ReadValueFromJson(themeId.ToString(), default(TerminalTheme))).Returns(value: null);
            var applicationDataContainers = new ApplicationDataContainers
            {
                Themes = themesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetTheme(themeId);

            result.Should().BeNull();
            themesContainer.Verify(x => x.ReadValueFromJson(themeId.ToString(), default(TerminalTheme)), Times.Once);
        }

        [Fact]
        public void GetTerminalOptions_Default_ReturnsTerminalOptionsFromRoamingSettings()
        {
            var terminalOptions = _fixture.Create<TerminalOptions>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            roamingSettings.Setup(x => x.ReadValueFromJson(nameof(TerminalOptions), It.IsAny<TerminalOptions>())).Returns(terminalOptions);
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetTerminalOptions();

            result.Should().BeEquivalentTo(terminalOptions);
            roamingSettings.Verify(x => x.ReadValueFromJson(nameof(TerminalOptions), It.IsAny<TerminalOptions>()), Times.Once);
        }

        [Fact]
        public void SaveTerminalOptions_Default_WritesToRoamingSettings()
        {
            var terminalOptions = _fixture.Create<TerminalOptions>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.SaveTerminalOptions(terminalOptions);

            roamingSettings.Verify(x => x.WriteValueAsJson(nameof(TerminalOptions), terminalOptions), Times.Once);
        }

        [Fact]
        public void SaveTerminalOptions_Default_InvokesTerminalOptionsChangedEvent()
        {
            var terminalOptions = _fixture.Create<TerminalOptions>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var terminalOptionsChangedEventInvoked = false;
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);
            settingsService.TerminalOptionsChanged += (s, e) => terminalOptionsChangedEventInvoked = true;

            settingsService.SaveTerminalOptions(terminalOptions);

            terminalOptionsChangedEventInvoked.Should().BeTrue();
        }

        [Fact]
        public void GetApplicationSettings_Default_ReturnsApplicationSettingsFromRoamingSettings()
        {
            var applicationSettings = _fixture.Create<ApplicationSettings>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            roamingSettings.Setup(x => x.ReadValueFromJson(nameof(ApplicationSettings), It.IsAny<ApplicationSettings>())).Returns(applicationSettings);
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetApplicationSettings();

            result.Should().BeEquivalentTo(applicationSettings);
            roamingSettings.Verify(x => x.ReadValueFromJson(nameof(ApplicationSettings), It.IsAny<ApplicationSettings>()), Times.Once);
        }

        [Fact]
        public void SaveApplicationSettings_Default_WritesToRoamingSettings()
        {
            var applicationSettings = _fixture.Create<ApplicationSettings>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.SaveApplicationSettings(applicationSettings);

            roamingSettings.Verify(x => x.WriteValueAsJson(nameof(ApplicationSettings), applicationSettings), Times.Once);
        }

        [Fact]
        public void SaveApplicationSettings_Default_InvokesApplicationSettingsChangedEvent()
        {
            var applicationSettings = _fixture.Create<ApplicationSettings>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var applicationSettingsChangedEventInvoked = false;
            var applicationDataContainers = new ApplicationDataContainers
            {
                RoamingSettings = roamingSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);
            Messenger.Default.Register<ApplicationSettingsChangedMessage>(this,
                a => applicationSettingsChangedEventInvoked = true);

            settingsService.SaveApplicationSettings(applicationSettings);

            applicationSettingsChangedEventInvoked.Should().BeTrue();
        }

        [Fact]
        public void GetKeyBindings_KeyBindingsInKeyBindingsContainer_ReturnsKeyBindingsFromKeyBindingsContainer()
        {
            var commands = _fixture.CreateMany<Command>(5);
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var keyBindingsContainer = new Mock<IApplicationDataContainer>();
            foreach (var command in commands)
            {
                keyBindingsContainer.Setup(x => x.ReadValueFromJson<Collection<KeyBinding>>(command.ToString(), null)).Returns(new Collection<KeyBinding> { });
            }
            var applicationDataContainers = new ApplicationDataContainers
            {
                KeyBindings = keyBindingsContainer.Object,
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetCommandKeyBindings();

            foreach (var command in commands)
            {
                keyBindingsContainer.Verify(x => x.ReadValueFromJson<Collection<KeyBinding>>(command.ToString(), null), Times.Once);
            }
        }

        [Fact]
        public void GetKeyBindings_KeyBindingsNotInKeyBindingsContainer_ReturnsKeyBindingsFromDefaultValueProvider()
        {
            var commands = _fixture.CreateMany<Command>(5);
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            var keyBindingsContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                KeyBindings = keyBindingsContainer.Object,
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            var result = settingsService.GetCommandKeyBindings();

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                defaultValueProvider.Verify(x => x.GetDefaultKeyBindings(command), Times.Once);
            }
        }

        [Fact]
        public void SaveKeyBindings_Default_WritesToKeyBindingsContainer()
        {
            var command = _fixture.Create<Command>();
            var keyBindings = _fixture.Create<ICollection<KeyBinding>>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var keyBindingsContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                KeyBindings = keyBindingsContainer.Object,
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.SaveKeyBindings(command.ToString(), keyBindings);

            keyBindingsContainer.Verify(x => x.WriteValueAsJson(command.ToString(), keyBindings), Times.Once);
        }

        [Fact]
        public void SaveKeyBindings_Default_InvokesKeyBindingsChangedEvent()
        {
            var command = _fixture.Create<Command>();
            var keyBindings = _fixture.Create<Collection<KeyBinding>>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var keyBindingsContainer = new Mock<IApplicationDataContainer>();
            var keyBindingsChangedEventInvoked = false;
            var applicationDataContainers = new ApplicationDataContainers
            {
                KeyBindings = keyBindingsContainer.Object,
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);
            Messenger.Default.Register<KeyBindingsChangedMessage>(this,
                message => keyBindingsChangedEventInvoked = true);

            settingsService.SaveKeyBindings(command.ToString(), keyBindings);

            keyBindingsChangedEventInvoked.Should().BeTrue();
        }

        [Fact]
        public void ResetKeyBindings_Default_WritesToKeyBindingsContainerFromDefaultValueProvider()
        {
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            var keyBindingsContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                KeyBindings = keyBindingsContainer.Object,
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            settingsService.ResetKeyBindings();

            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                keyBindingsContainer.Verify(x => x.WriteValueAsJson(command.ToString(), It.IsAny<ICollection<KeyBinding>>()), Times.Once);
                defaultValueProvider.Verify(x => x.GetDefaultKeyBindings(command), Times.Once);
            }
        }

        [Fact]
        public void ResetKeyBindings_Default_InvokesKeyBindingsChangedEvent()
        {
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var keyBindingsContainer = new Mock<IApplicationDataContainer>();
            var keyBindingsChangedEventInvoked = false;
            var applicationDataContainers = new ApplicationDataContainers
            {
                KeyBindings = keyBindingsContainer.Object,
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);
            Messenger.Default.Register<KeyBindingsChangedMessage>(this,
                message => keyBindingsChangedEventInvoked = true);

            settingsService.ResetKeyBindings();

            keyBindingsChangedEventInvoked.Should().BeTrue();
        }

        [Fact]
        public void GetDefaultShellProfileId_ValueExistsInRoamingSettings_ReturnsDefaultShellProfileId()
        {
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var localSettings = new Mock<IApplicationDataContainer>();
            var defaultShellProfileId = (object)_fixture.Create<Guid>();
            localSettings.Setup(x => x.TryGetValue(SettingsService.DefaultShellProfileKey, out defaultShellProfileId)).Returns(true);
            var applicationDataContainers = new ApplicationDataContainers
            {
                LocalSettings = localSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetDefaultShellProfileId();

            result.Should().Be((Guid)defaultShellProfileId);
            localSettings.Verify(x => x.TryGetValue(SettingsService.DefaultShellProfileKey, out defaultShellProfileId), Times.Once);
        }

        [Fact]
        public void GetDefaultShellProfileId_ValueDoesNotExistInRoamingSettings_ReturnsDefaultShellProfileIdFromDefaultValueProvider()
        {
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            var defaultShellProfileId = _fixture.Create<Guid>();
            defaultValueProvider.Setup(x => x.GetDefaultShellProfileId()).Returns(defaultShellProfileId);
            var localSettings = new Mock<IApplicationDataContainer>();
            var currentDefaultShellProfileId = (object)_fixture.Create<Guid>();
            localSettings.Setup(x => x.TryGetValue(SettingsService.DefaultShellProfileKey, out currentDefaultShellProfileId)).Returns(false);
            var applicationDataContainers = new ApplicationDataContainers
            {
                LocalSettings = localSettings.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            var result = settingsService.GetDefaultShellProfileId();

            result.Should().Be(defaultShellProfileId);
            defaultValueProvider.Verify(x => x.GetDefaultShellProfileId(), Times.Once);
            localSettings.Verify(x => x.TryGetValue(SettingsService.DefaultShellProfileKey, out currentDefaultShellProfileId), Times.Once);
        }

        [Fact]
        public void GetShellProfiles_Default_CallsGetAllOnShellProfilesContainer()
        {
            var shellProfiles = _fixture.CreateMany<ShellProfile>(3);
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var shellProfilesContainer = new Mock<IApplicationDataContainer>();
            shellProfilesContainer.Setup(x => x.GetAll()).Returns(shellProfiles.Select(JsonConvert.SerializeObject));
            var applicationDataContainers = new ApplicationDataContainers
            {
                ShellProfiles = shellProfilesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            var result = settingsService.GetShellProfiles();

            result.Select(p => p.EqualTo(shellProfiles.FirstOrDefault(x => x.Id == p.Id)).Should().BeTrue());
            shellProfilesContainer.Verify(x => x.GetAll(), Times.Once);
        }

        [Fact]
        public void SaveShellProfile_Default_WritesToShellProfilesContainer()
        {
            var shellProfile = _fixture.Create<ShellProfile>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var shellProfilesContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                ShellProfiles = shellProfilesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.SaveShellProfile(shellProfile);

            shellProfilesContainer.Verify(x => x.WriteValueAsJson(shellProfile.Id.ToString(), shellProfile), Times.Once);
        }

        [Fact]
        public void DeleteShellProfile_Default_CallsDeleteOnShellProfilesContainer()
        {
            var shellProfileId = _fixture.Create<Guid>();
            var defaultValueProvider = Mock.Of<IDefaultValueProvider>();
            var shellProfilesContainer = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                ShellProfiles = shellProfilesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            var settingsService = new SettingsService(defaultValueProvider, applicationDataContainers);

            settingsService.DeleteShellProfile(shellProfileId);

            shellProfilesContainer.Verify(x => x.Delete(shellProfileId.ToString()), Times.Once);
        }

        [Fact]
        public void GetDefaultShellProfile_ProfileNotFound_DefaultShellProfileIdGetsResetToDefault()
        {
            var currentShellProfileId = (object)_fixture.Create<Guid>();
            var defaultShellProfile = _fixture.Create<ShellProfile>();
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            defaultValueProvider.Setup(x => x.GetDefaultShellProfileId()).Returns(defaultShellProfile.Id);
            var shellProfilesContainer = new Mock<IApplicationDataContainer>();
            var localSettings = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                ShellProfiles = shellProfilesContainer.Object,
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = localSettings.Object,
                Themes = Mock.Of<IApplicationDataContainer>()
            };
            shellProfilesContainer.Setup(x => x.ReadValueFromJson(currentShellProfileId.ToString(), default(ShellProfile))).Returns(value: null);
            shellProfilesContainer.Setup(x => x.ReadValueFromJson(defaultShellProfile.Id.ToString(), default(ShellProfile))).Returns(defaultShellProfile);
            localSettings.Setup(x => x.TryGetValue(SettingsService.DefaultShellProfileKey, out currentShellProfileId)).Returns(true);
            var settingsService = new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            var result = settingsService.GetDefaultShellProfile();

            result.Should().Be(defaultShellProfile);
            localSettings.Verify(x => x.SetValue(SettingsService.DefaultShellProfileKey, defaultShellProfile.Id), Times.Once);
        }

        [Fact]
        public void GetCurrentTheme_ThemeNotFound_CurrentThemeIdGetsResetToDefault()
        {
            var currentThemeId = (object)_fixture.Create<Guid>();
            var defaultTheme = _fixture.Create<TerminalTheme>();
            var defaultValueProvider = new Mock<IDefaultValueProvider>();
            defaultValueProvider.Setup(x => x.GetDefaultThemeId()).Returns(defaultTheme.Id);
            var themesContainer = new Mock<IApplicationDataContainer>();
            var roamingSettings = new Mock<IApplicationDataContainer>();
            var applicationDataContainers = new ApplicationDataContainers
            {
                ShellProfiles = Mock.Of<IApplicationDataContainer>(),
                KeyBindings = Mock.Of<IApplicationDataContainer>(),
                LocalSettings = Mock.Of<IApplicationDataContainer>(),
                RoamingSettings = roamingSettings.Object,
                Themes = themesContainer.Object
            };
            themesContainer.Setup(x => x.ReadValueFromJson(currentThemeId.ToString(), default(TerminalTheme))).Returns(value: null);
            themesContainer.Setup(x => x.ReadValueFromJson(defaultTheme.Id.ToString(), default(TerminalTheme))).Returns(defaultTheme);
            roamingSettings.Setup(x => x.TryGetValue(SettingsService.CurrentThemeKey, out currentThemeId)).Returns(true);
            var settingsService = new SettingsService(defaultValueProvider.Object, applicationDataContainers);

            var result = settingsService.GetCurrentTheme();

            result.Should().Be(defaultTheme);
            roamingSettings.Verify(x => x.SetValue(SettingsService.CurrentThemeKey, defaultTheme.Id), Times.Once);
        }
    }
}