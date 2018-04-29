using System.Collections.Generic;

namespace FluentTerminal.Models.Requests
{
    public class SetToggleWindowKeyBindingsRequest
    {
        public IEnumerable<KeyBinding> KeyBindings { get; set; }
    }
}
