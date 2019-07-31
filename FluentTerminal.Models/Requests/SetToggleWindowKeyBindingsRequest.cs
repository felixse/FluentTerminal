using System.Collections.Generic;

namespace FluentTerminal.Models.Requests
{
    public class SetToggleWindowKeyBindingsRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.SetToggleWindowKeyBindingsRequest;

        public IEnumerable<KeyBinding> KeyBindings { get; set; }
    }
}