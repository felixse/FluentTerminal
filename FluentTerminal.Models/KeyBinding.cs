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

        public Dictionary<string, object> ToDict(AbstractCommand command)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            string commandString = command.ToString();
            commandString = commandString[0].ToString().ToLower() + commandString.Substring(1);
            ret.Add("command", commandString);
            ret.Add("key", Key);
            ret.Add("ctrl", Ctrl);
            ret.Add("alt", Alt);
            ret.Add("shift", Shift);
            ret.Add("meta", Meta);

            return ret;
        }
    }
}