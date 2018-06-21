﻿using System;
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

        public void SendCommand(Command command)
        {
            if (_commandHandlers.TryGetValue(command, out Action handler))
            {
                handler.Invoke();
                return;
            }

            // already registered by SystemTray's ToggleWindowService
            if (command == Command.ToggleWindow)
            {
                return;
            }

            throw new KeyNotFoundException($"No handler registered for command: {command}");
        }
    }
}
