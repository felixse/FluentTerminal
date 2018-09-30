using FluentTerminal.Models.Enums;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services.Implementation
{
    public class KeyboardCommandService : IKeyboardCommandService
    {
        private readonly Dictionary<AbstractCommand, Action> _commandHandlers = new Dictionary<AbstractCommand, Action>();

        public void RegisterCommandHandler(AbstractCommand command, Action handler)
        {
            if (_commandHandlers.ContainsKey(command))
            {
                throw new InvalidOperationException("command already registered");
            }

            _commandHandlers[command] = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void SendCommand(AbstractCommand command)
        {
            if (_commandHandlers.TryGetValue(command, out Action handler))
            {
                handler.Invoke();
                return;
            }

            // already registered by SystemTray's ToggleWindowService
            if (command.Equals(Command.ToggleWindow))
            {
                return;
            }

            throw new KeyNotFoundException($"No handler registered for command: {command}");
        }
    }
}