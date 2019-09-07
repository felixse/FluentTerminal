using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services.Implementation
{
    public class KeyboardCommandService : IKeyboardCommandService
    {
        private readonly Dictionary<string, Action> _commandHandlers = new Dictionary<string, Action>();

        public void RegisterCommandHandler(string command, Action handler)
        {
            if (_commandHandlers.ContainsKey(command))
            {
                throw new InvalidOperationException("command already registered");
            }

            _commandHandlers[command] = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void DeregisterCommandHandler(string command)
        {
            if (_commandHandlers.ContainsKey(command))
            {
                _commandHandlers.Remove(command);
            }
        }

        public void SendCommand(string command)
        {
            if (_commandHandlers.TryGetValue(command, out Action handler))
            {
                handler.Invoke();
                return;
            }

            // already registered by SystemTray's ToggleWindowService
            if (command == nameof(Command.ToggleWindow))
            {
                return;
            }

            throw new KeyNotFoundException($"No handler registered for command: {command}");
        }

        public void ClearCommandHandlers()
        {
            _commandHandlers.Clear();
        }
    }
}