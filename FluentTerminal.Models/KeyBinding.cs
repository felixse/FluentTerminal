﻿using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class KeyBinding
    {
        public AppCommand Command { get; set; }
        public int Key { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Meta { get; set; }
    }
}