using System.Collections.Generic;

namespace FluentTerminal.Models
{
    public class KeyBindings
    {
        public ICollection<KeyBinding> ToggleWindow { get; set; }
        public ICollection<KeyBinding> NextTab { get; set; }
        public ICollection<KeyBinding> PreviousTab { get; set; }
        public ICollection<KeyBinding> NewTab { get; set; }
        public ICollection<KeyBinding> ConfigurableNewTab { get; set; }
        public ICollection<KeyBinding> ChangeTabTitle { get; set; }
        public ICollection<KeyBinding> CloseTab { get; set; }
        public ICollection<KeyBinding> NewWindow { get; set; }
        public ICollection<KeyBinding> ConfigurableNewWindow { get; set; }
        public ICollection<KeyBinding> ShowSettings { get; set; }
        public ICollection<KeyBinding> Copy { get; set; }
        public ICollection<KeyBinding> Paste { get; set; }
        public ICollection<KeyBinding> Search { get; set; }
        public ICollection<KeyBinding> SavedSshNewTab { get; set; }
        public ICollection<KeyBinding> SavedSshNewWindow { get; set; }
    }
}