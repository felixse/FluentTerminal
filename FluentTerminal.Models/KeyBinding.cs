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
    }
}