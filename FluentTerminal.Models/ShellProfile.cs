using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentTerminal.Models
{
    public class ShellProfile
    {
        public ShellProfile()
        {

        }

        public ShellProfile(ShellProfile other)
        {
            Id = other.Id;
            PreInstalled = other.PreInstalled;
            Name = other.Name;
            Arguments = other.Arguments;
            Location = other.Location;
            WorkingDirectory = other.WorkingDirectory;
            TabThemeId = other.TabThemeId;
            TerminalThemeId = other.TerminalThemeId;
            KeyBindings = other.KeyBindings.Select(x => new KeyBinding(x)).ToList();
        }

        public Guid Id { get; set; }
        public bool PreInstalled { get; set; }
        public string Name { get; set; }
        public string Arguments { get; set; }
        public string Location { get; set; }
        public string WorkingDirectory { get; set; }
        public int TabThemeId { get; set; }
        public Guid TerminalThemeId { get; set; }
        public ICollection<KeyBinding> KeyBindings { get; set; } = new List<KeyBinding>();
    }
}