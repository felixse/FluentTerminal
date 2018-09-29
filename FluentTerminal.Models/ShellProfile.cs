using System;
using System.Collections.Generic;

namespace FluentTerminal.Models
{
    public class ShellProfile
    {
        public Guid Id { get; set; }
        public bool PreInstalled { get; set; }
        public string Name { get; set; }
        public string Arguments { get; set; }
        public string Location { get; set; }
        public string WorkingDirectory { get; set; }
        public int TabThemeId { get; set; }
        public ICollection<KeyBinding> KeyBinding { get; set; }
    }
}