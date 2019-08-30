using System;

namespace FluentTerminal.Models
{
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

        public string Command { get; set; }
        public int Key { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
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

        public override int GetHashCode()
        {
            return HashCode.Combine(Command, Key, Ctrl, Alt, Shift, Meta);
        }
    }
}