using System.Runtime.Serialization;

namespace FluentTerminal.Models
{
    [DataContract]
    public class KeyBinding
    {
        public KeyBinding()
        {

        }

        public KeyBinding(KeyBinding other)
        {
            Command = other.Command;
            Key = other.Key;
            Ctrl = other.Ctrl;
            Alt = other.Alt;
            Shift = other.Shift;
            Meta = other.Meta;
        }

        [DataMember(Order = 0)]
        public string Command { get; set; }

        [DataMember(Order = 1)]
        public int Key { get; set; }

        [DataMember(Order = 2)]
        public bool Ctrl { get; set; }

        [DataMember(Order = 3)]
        public bool Alt { get; set; }

        [DataMember(Order = 4)]
        public bool Shift { get; set; }

        [DataMember(Order = 5)]
        public bool Meta { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is KeyBinding other)
            {
                return other.Command.Equals(Command)
                    && other.Key == Key
                    && other.Ctrl == Ctrl
                    && other.Alt == Alt
                    && other.Shift == Shift
                    && other.Meta == Meta;
            }
            return false;
        }
    }
}