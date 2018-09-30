using System.Collections.Generic;

namespace FluentTerminal.Models
{
    public class KeyBinding
    {
        public int Key { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Meta { get; set; }

        public Dictionary<string, object> ToDict(Command command)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("command", command.ToString());
            ret.Add("key", Key);
            ret.Add("ctrl", Ctrl);
            ret.Add("alt", Alt);
            ret.Add("shift", Shift);
            ret.Add("meta", Meta);

            return ret;
        }
    }

    //public class PairedKeyBinding
    //{
    //    // TODO References to this should probably be replaced with straight up dictionaries that map to what we care about.
    //    public string Command { get; set; }
    //    public int Key { get; set; }
    //    public bool Ctrl { get; set; }
    //    public bool Alt { get; set; }
    //    public bool Shift { get; set; }
    //    public bool Meta { get; set; }

    //    public PairedKeyBinding(ICommand command, KeyBinding keyBinding)
    //    {
    //        Command = command.ToString();
    //        Key = keyBinding.Key;
    //        Ctrl = keyBinding.Ctrl;
    //        Alt = keyBinding.Alt;
    //        Shift = keyBinding.Shift;
    //        Meta = keyBinding.Meta;
    //    }
    //}
}