using FluentTerminal.Models.Enums;
using System;

namespace FluentTerminal.App.Services
{
    public interface IKeyboardCommandService
    {
        void RegisterCommandHandler(Command command, Action handler);
        void DeegisterCommandHandler(Command command);
        void SendCommand(Command command);
    }
}
