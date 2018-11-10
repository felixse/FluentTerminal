using System;

namespace FluentTerminal.App.Services
{
    public interface IKeyboardCommandService
    {
        void RegisterCommandHandler(string command, Action handler);

        void SendCommand(string command);
        void DeregisterCommandHandler(string command);
    }
}