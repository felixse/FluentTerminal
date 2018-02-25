using FluentTerminal.Models.Enums;
using Windows.System;

namespace FluentTerminal.Models
{
    public class KeyBinding
    {
        public Command Command { get; set; }
        public int Key { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
    }
}
