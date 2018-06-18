using System;
using System.Collections.Generic;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services.Implementation
{
    public class KeyboardCommandService : IKeyboardCommandService
    {
        private readonly Dictionary<Command, Action> _commandHandlers = new Dictionary<Command, Action>();

        public void RegisterCommandHandler(Command command, Action handler)
        {
            if (_commandHandlers.ContainsKey(command))
            {
                throw new InvalidOperationException("command already registered");
            }

            _commandHandlers[command] = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void DeegisterCommandHandler(Command command)
        {
            if (_commandHandlers.ContainsKey(command))
            {
                _commandHandlers.Remove(command);
            }
            else
            {
                throw new InvalidOperationException("command not registered");
            }
        }

        public void SendCommand(Command command)
        {
            if (_commandHandlers.TryGetValue(command, out Action handler))
            {
                handler.Invoke();
                return;
            }
            throw new KeyNotFoundException($"No handler registered for command: {command}");
        }
    }
}
