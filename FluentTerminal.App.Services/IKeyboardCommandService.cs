﻿using FluentTerminal.Models.Enums;
using FluentTerminal.Models;
using System;

namespace FluentTerminal.App.Services
{
    public interface IKeyboardCommandService
    {
        void RegisterCommandHandler(Command command, Action handler);

        void SendCommand(Command command);
    }
}