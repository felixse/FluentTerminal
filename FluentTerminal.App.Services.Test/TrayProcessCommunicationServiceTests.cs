using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Responses;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class TrayProcessCommunicationServiceTests
    {
        private readonly Fixture _fixture;

        public TrayProcessCommunicationServiceTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateTerminal_Default_ReturnsResponseFromAppServiceConnection()
        {
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetKeyBindings()).Returns(new Dictionary<Command, ICollection<KeyBinding>>
            {
                [Command.ToggleWindow] = keyBindings.ToList()
            });
            var appServiceConnection = new Mock<IAppServiceConnection>();
            var response = _fixture.Create<CreateTerminalResponse>();
            var responseMessage = new Dictionary<string, string>
            {
                [MessageKeys.Type] = _fixture.Create<string>(),
                [MessageKeys.Content] = JsonConvert.SerializeObject(response)
            };
            appServiceConnection.Setup(x => x.SendMessageAsync(It.IsAny<IDictionary<string, string>>())).Returns(Task.FromResult((IDictionary<string, string>)responseMessage));
            var terminalSize = _fixture.Create<TerminalSize>();
            var shellProfile = _fixture.Create<ShellProfile>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            await trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            var result = await trayProcessCommunicationService.CreateTerminal(terminalSize, shellProfile);

            result.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Initialize_Default_SendsSetToggleWindowKeyBindingsRequest()
        {
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetKeyBindings()).Returns(new Dictionary<Command, ICollection<KeyBinding>>
            {
                [Command.ToggleWindow] = keyBindings.ToList()
            });
            var appServiceConnection = new Mock<IAppServiceConnection>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);

            await trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            appServiceConnection.Verify(x => x.SendMessageAsync(It.Is<IDictionary<string, string>>(d => d[MessageKeys.Type] == MessageTypes.SetToggleWindowKeyBindingsRequest)), Times.Once);
        }

        [Fact]
        public async Task ResizeTerminal_Default_SendsResizeTerminalRequest()
        {
            var terminalId = _fixture.Create<int>();
            var terminalSize = _fixture.Create<TerminalSize>();
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetKeyBindings()).Returns(new Dictionary<Command, ICollection<KeyBinding>>
            {
                [Command.ToggleWindow] = keyBindings.ToList()
            });
            var appServiceConnection = new Mock<IAppServiceConnection>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            await trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            await trayProcessCommunicationService.ResizeTerminal(terminalId, terminalSize);

            appServiceConnection.Verify(x => x.SendMessageAsync(It.Is<IDictionary<string, string>>(d => d[MessageKeys.Type] == MessageTypes.ResizeTerminalRequest)), Times.Once);
        }
    }
}
