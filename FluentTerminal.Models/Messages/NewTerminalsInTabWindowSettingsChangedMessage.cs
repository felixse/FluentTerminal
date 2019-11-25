using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models.Messages
{
    public class NewTerminalsInTabWindowSettingsChangedMessage
    {
        public NewTerminalLocation Location { get; }

        public NewTerminalsInTabWindowSettingsChangedMessage(NewTerminalLocation location) => Location = location;
    }
}