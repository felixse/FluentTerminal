using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class DefaultValueProviderTests
    {
        public static IEnumerable<object[]> Commands => System.Enum.GetValues(typeof(Command)).Cast<Command>().Select(x => new object[] { x });

        [Fact]
        public void GetDefaultApplicationSettings_Default_DoesNotReturnNull()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var applicationSettings = defaultValueProvider.GetDefaultApplicationSettings();

            applicationSettings.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(Commands))]
        public void GetDefaultKeyBindings_AllCommands_ReturnsANonEmptyList(Command command)
        {
            var defaultValueProvider = new DefaultValueProvider();

            var keyBindings = defaultValueProvider.GetDefaultKeyBindings(command);

            keyBindings.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetDefaultShellProfileId_Default_DoesNotReturnEmptyGuid()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var defaultShellProfileId = defaultValueProvider.GetDefaultShellProfileId();

            defaultShellProfileId.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDefaultShellProfileId_Default_IdIsOneOfThePreInstalledShellProfiles()
        {
            var defaultValueProvider = new DefaultValueProvider();
            var shellProfiles = defaultValueProvider.GetPreinstalledShellProfiles();

            var defaultShellProfileId = defaultValueProvider.GetDefaultShellProfileId();

            shellProfiles.Select(x => x.Id).Should().Contain(defaultShellProfileId);
        }

        [Fact]
        public void GetDefaultTerminalOptions_Default_DoesNotReturnNull()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var defaultTerminalOptions = defaultValueProvider.GetDefaultTerminalOptions();

            defaultTerminalOptions.Should().NotBeNull();
        }

        [Fact]
        public void GetDefaultThemeId_Default_DoesNotReturnEmptyGuid()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var defaultThemeId = defaultValueProvider.GetDefaultThemeId();

            defaultThemeId.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDefaultThemeId_Default_IdIsOneOfThePreInstalledThemes()
        {
            var defaultValueProvider = new DefaultValueProvider();
            var themes = defaultValueProvider.GetPreInstalledThemes();

            var defaultThemeId = defaultValueProvider.GetDefaultThemeId();

            themes.Select(x => x.Id).Should().Contain(defaultThemeId);
        }

        [Fact]
        public void GetPreinstalledShellProfiles_Default_ReturnsANonEmptyList()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var shellProfiles = defaultValueProvider.GetPreinstalledShellProfiles();

            shellProfiles.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetPreinstalledShellProfiles_Default_HaveUniqueIds()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var shellProfiles = defaultValueProvider.GetPreinstalledShellProfiles();

            shellProfiles.Select(x => x.Id).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void GetPreinstalledShellProfiles_Default_HavePreInstalledSetToTrue()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var shellProfiles = defaultValueProvider.GetPreinstalledShellProfiles();

            shellProfiles.Select(x => x.PreInstalled).Should().AllBeEquivalentTo(true);
        }

        [Fact]
        public void GetPreinstalledShellProfiles_Default_HaveNonEmptyLocations()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var shellProfiles = defaultValueProvider.GetPreinstalledShellProfiles();

            shellProfiles.Select(x => x.Location).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetPreInstalledThemes_Default_ReturnsANonEmptyList()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var themes = defaultValueProvider.GetPreInstalledThemes();

            themes.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetPreinstalledThemes_Default_HaveUniqueIds()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var themes = defaultValueProvider.GetPreInstalledThemes();

            themes.Select(x => x.Id).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void GetPreinstalledThemes_Default_HavePreInstalledSetToTrue()
        {
            var defaultValueProvider = new DefaultValueProvider();

            var themes = defaultValueProvider.GetPreInstalledThemes();

            themes.Select(x => x.PreInstalled).Should().AllBeEquivalentTo(true);
        }
    }
}