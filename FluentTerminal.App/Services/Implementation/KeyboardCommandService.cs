using System;
using System.Collections.Generic;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services.Implementation
{
    public class KeyboardCommandService : IKeyboardCommandService
    {
        private Dictionary<Command, Action> _commandHandlers = new Dictionary<Command, Action>();

        public void RegisterCommandHandler(Command command, Action handler)
        {
            _commandHandlers[command] = handler;
        }

        public void SendCommand(Command command)
        {
            if (_commandHandlers.TryGetValue(command, out Action handler))
            {
                handler.Invoke();
                return;
            }
            throw new Exception($"No handler registered for command: {command}");
        }
    }
}
