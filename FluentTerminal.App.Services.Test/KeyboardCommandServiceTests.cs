﻿using AutoFixture;
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
            var command = ((Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length)).ToString();
            Action handler = null;
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("handler");
        }

        [Fact]
        public void RegisterCommandHandler_CommandAlreadyRegisted_ThrowsInvalidOperationException()
        {
            var command = ((Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length)).ToString();
            var handler = _fixture.Create<Action>();
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, handler);

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<InvalidOperationException>();
        }
        
        [Fact]
        public void DeregisterCommandHandler_CommandIsNull_ThrowsArgumentNullException()
        {
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.DeregisterCommandHandler(null);

            invoke.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DeregisterCommandHandler_CommandRegistered_HandlerGetsRemoved()
        {
            var command = ((Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length)).ToString();
            var handler = _fixture.Create<Action>();
            var keyboardCommandService = new KeyboardCommandService();

            keyboardCommandService.RegisterCommandHandler(command, handler);

            Action invoke = () => keyboardCommandService.DeregisterCommandHandler(command);

            invoke.Should().NotThrow();
        }

        [Fact]
        public void DeregisterCommandHandler_CommandNotRegistered_ShouldNotThrow()
        {
            var command = ((Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length)).ToString();
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.DeregisterCommandHandler(command);

            invoke.Should().NotThrow();
        }

        [Fact]
        public void SendCommand_CommandIsRegistered_HandlerGetsInvoked()
        {
            var command = ((Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length)).ToString();
            var handlerCalled = false;
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, () => handlerCalled = true);

            keyboardCommandService.SendCommand(command);

            handlerCalled.Should().BeTrue();
        }

        [Fact]
        public void SendCommand_CommandIsNotRegisted_KeyNotFoundExceptionIsThrown()
        {
            var command = ((Command)new Random().Next(1, Enum.GetValues(typeof(Command)).Length)).ToString();
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.SendCommand(command);

            invoke.Should().Throw<KeyNotFoundException>();
        }
        
        [Fact]
        public void SendCommand_CommandIsToggleWindow_ShouldNotThrow()
        {
            var command = nameof(Command.ToggleWindow);
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.SendCommand(command);

            invoke.Should().NotThrow();
        }
    }
}
