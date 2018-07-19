using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class KeyboardCommandServiceTests
    {
        private readonly Fixture _fixture;

        public KeyboardCommandServiceTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void RegisterCommandHandler_HandlerIsNull_ThrowsArgumentNullException()
        {
            var command = (Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length);
            Action handler = null;
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("handler");
        }

        [Fact]
        public void RegisterCommandHandler_CommandAlreadyRegisted_ThrowsInvalidOperationException()
        {
            var command = (Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length);
            var handler = _fixture.Create<Action>();
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, handler);

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SendCommand_CommandIsRegistered_HandlerGetsInvoked()
        {
            var command = (Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length);
            var handlerCalled = false;
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, () => handlerCalled = true);

            keyboardCommandService.SendCommand(command);

            handlerCalled.Should().BeTrue();
        }

        [Fact]
        public void SendCommand_CommandIsNotRegisted_KeyNotFoundExceptionIsThrown()
        {
            var command = (Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length);
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.SendCommand(command);

            invoke.Should().Throw<KeyNotFoundException>();
        }
    }
}