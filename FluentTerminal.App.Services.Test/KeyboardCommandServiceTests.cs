using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models;
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
            var command = (AppCommand)new Random().Next(1, Enum.GetValues(typeof(AppCommand)).Length);
            Action handler = null;
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("handler");
        }

        [Fact]
        public void RegisterCommandHandler_CommandAlreadyRegisted_ThrowsInvalidOperationException()
        {
            var command = (AppCommand)new Random().Next(1, Enum.GetValues(typeof(AppCommand)).Length);
            var handler = _fixture.Create<Action>();
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, handler);

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SendCommand_CommandIsRegistered_HandlerGetsInvoked()
        {
            var command = (AppCommand)new Random().Next(1, Enum.GetValues(typeof(AppCommand)).Length);
            var handlerCalled = false;
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, () => handlerCalled = true);

            keyboardCommandService.SendCommand(command);

            handlerCalled.Should().BeTrue();
        }

        [Fact]
        public void SendCommand_CommandIsNotRegisted_KeyNotFoundExceptionIsThrown()
        {
            var command = (AppCommand)new Random().Next(1, Enum.GetValues(typeof(AppCommand)).Length);
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.SendCommand(command);

            invoke.Should().Throw<KeyNotFoundException>();
        }
    }
}