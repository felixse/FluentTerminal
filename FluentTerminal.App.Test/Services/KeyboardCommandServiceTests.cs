using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Test.Services
{
    [TestClass]
    public class KeyboardCommandServiceTests
    {
        private Fixture _fixture;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture();
        }

        [TestMethod]
        public void RegisterCommandHandler_HandlerIsNull_ThrowsArgumentNullException()
        {
            var command = _fixture.Create<Command>();
            Action handler = null;
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("handler");
        }

        [TestMethod]
        public void RegisterCommandHandler_CommandAlreadyRegisted_ThrowsInvalidOperationException()
        {
            var command = _fixture.Create<Command>();
            var handler = _fixture.Create<Action>();
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, handler);

            Action invoke = () => keyboardCommandService.RegisterCommandHandler(command, handler);

            invoke.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void SendCommand_CommandIsRegistered_HandlerGetsInvoked()
        {
            var command = _fixture.Create<Command>();
            var handlerCalled = false;
            var keyboardCommandService = new KeyboardCommandService();
            keyboardCommandService.RegisterCommandHandler(command, () => handlerCalled = true);

            keyboardCommandService.SendCommand(command);

            handlerCalled.Should().BeTrue();
        }

        [TestMethod]
        public void SendCommand_CommandIsNotRegisted_KeyNotFoundExceptionIsThrown()
        {
            var command = _fixture.Create<Command>();
            var keyboardCommandService = new KeyboardCommandService();

            Action invoke = () => keyboardCommandService.SendCommand(command);

            invoke.Should().Throw<KeyNotFoundException>();
        }
    }
}
