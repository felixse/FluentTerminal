using System.Runtime.Serialization;

namespace FluentTerminal.Models
{
    [DataContract]
    public class TerminalSize
    {
        [DataMember(Order = 0)]
        public int Columns { get; set; }

        [DataMember(Order = 1)]
        public int Rows { get; set; }
    }
}