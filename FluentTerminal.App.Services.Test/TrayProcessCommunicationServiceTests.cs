using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
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
            settingsService.Setup(x => x.GetCommandKeyBindings()).Returns(new Dictionary<string, ICollection<KeyBinding>>
            {
                [nameof(Command.ToggleWindow)] = keyBindings.ToList()
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
            var sessionType = _fixture.Create<SessionType>();
            var id = _fixture.Create<int>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            var result = await trayProcessCommunicationService.CreateTerminal(id, terminalSize, shellProfile, sessionType);

            result.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ResizeTerminal_Default_SendsResizeTerminalRequest()
        {
            var terminalId = _fixture.Create<int>();
            var terminalSize = _fixture.Create<TerminalSize>();
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetCommandKeyBindings()).Returns(new Dictionary<string, ICollection<KeyBinding>>
            {
                [nameof(Command.ToggleWindow)] = keyBindings.ToList()
            });
            var appServiceConnection = new Mock<IAppServiceConnection>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            await trayProcessCommunicationService.ResizeTerminal(terminalId, terminalSize);

            appServiceConnection.Verify(x => x.SendMessageAsync(It.Is<IDictionary<string, string>>(d => d[MessageKeys.Type] == nameof(ResizeTerminalRequest))), Times.Once);
        }

        [Fact]
        public async Task Write_Default_SendsWriteDataRequest()
        {
            var terminalId = _fixture.Create<int>();
            var data = _fixture.Create<byte[]>();
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetCommandKeyBindings()).Returns(new Dictionary<string, ICollection<KeyBinding>>
            {
                [nameof(Command.ToggleWindow)] = keyBindings.ToList()
            });
            var appServiceConnection = new Mock<IAppServiceConnection>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            await trayProcessCommunicationService.Write(terminalId, data);

            appServiceConnection.Verify(x => x.SendMessageAsync(It.Is<IDictionary<string, string>>(d => d[MessageKeys.Type] == nameof(WriteDataRequest))), Times.Once);
        }

        [Fact]
        public async Task CloseTerminal_Default_SendsTerminalExitedRequest()
        {
            var terminalId = _fixture.Create<int>();
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetCommandKeyBindings()).Returns(new Dictionary<string, ICollection<KeyBinding>>
            {
                [nameof(Command.ToggleWindow)] = keyBindings.ToList()
            });
            var appServiceConnection = new Mock<IAppServiceConnection>();
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            trayProcessCommunicationService.Initialize(appServiceConnection.Object);

            await trayProcessCommunicationService.CloseTerminal(terminalId);

            appServiceConnection.Verify(x => x.SendMessageAsync(It.Is<IDictionary<string, string>>(d => d[MessageKeys.Type] == nameof(TerminalExitedRequest))), Times.Once);
        }

        [Fact]
        public void OnMessageReceived_TerminalExitedRequest_InvokesTerminalExitedEvent()
        {
            var terminalId = _fixture.Create<int>();
            var receivedTerminalId = 0;
            var terminalExitedEventCalled = false;
            var request = new TerminalExitedRequest
            {
                TerminalId = terminalId
            };
            var message = new Dictionary<string, string>
            {
                [MessageKeys.Type] = nameof(TerminalExitedRequest),
                [MessageKeys.Content] = JsonConvert.SerializeObject(request)
            };
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetCommandKeyBindings()).Returns(new Dictionary<string, ICollection<KeyBinding>>
            {
                [nameof(Command.ToggleWindow)] = keyBindings.ToList()
            });
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            var appServiceConnection = new Mock<IAppServiceConnection>();
            trayProcessCommunicationService.Initialize(appServiceConnection.Object);
            trayProcessCommunicationService.TerminalExited += (s, e) =>
            {
                terminalExitedEventCalled = true;
                receivedTerminalId = e;
            };

            appServiceConnection.Raise(x => x.MessageReceived += null, null, message);

            terminalExitedEventCalled.Should().BeTrue();
            receivedTerminalId.Should().Be(terminalId);
        }

        [Fact]
        public void OnMessageReceived_DisplayTerminalOutputRequest_InvokesCorrectOutputHandler()
        {
            var terminalId = _fixture.Create<int>();
            var output = _fixture.Create<byte[]>();
            var receivedOutput = default(byte[]);
            var request = new DisplayTerminalOutputRequest
            {
                TerminalId = terminalId,
                Output = output
            };
            var message = new Dictionary<string, string>
            {
                [MessageKeys.Type] = nameof(DisplayTerminalOutputRequest),
                [MessageKeys.Content] = JsonConvert.SerializeObject(request)
            };
            var settingsService = new Mock<ISettingsService>();
            var keyBindings = _fixture.CreateMany<KeyBinding>(3);
            settingsService.Setup(x => x.GetCommandKeyBindings()).Returns(new Dictionary<string, ICollection<KeyBinding>>
            {
                [nameof(Command.ToggleWindow)] = keyBindings.ToList()
            });
            var trayProcessCommunicationService = new TrayProcessCommunicationService(settingsService.Object);
            var appServiceConnection = new Mock<IAppServiceConnection>();
            trayProcessCommunicationService.Initialize(appServiceConnection.Object);
            trayProcessCommunicationService.SubscribeForTerminalOutput(terminalId, o =>
            {
                receivedOutput = o;
            });

            appServiceConnection.Raise(x => x.MessageReceived += null, null, message);

            receivedOutput.Should().BeEquivalentTo(output);
        }
    }
}