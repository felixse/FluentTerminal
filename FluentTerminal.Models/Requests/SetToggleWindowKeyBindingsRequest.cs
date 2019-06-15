using System.Collections.Generic;

namespace FluentTerminal.Models.Requests
{
    public class SetToggleWindowKeyBindingsRequest : IMessage
    {
        public const byte Identifier = 6;

        byte IMessage.Identifier => Identifier;

        public IEnumerable<KeyBinding> KeyBindings { get; set; }
    }
}