using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class SetToggleWindowKeyBindingsRequest : IMessage
    {
        public const byte Identifier = 4;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public IEnumerable<KeyBinding> KeyBindings { get; set; }
    }
}