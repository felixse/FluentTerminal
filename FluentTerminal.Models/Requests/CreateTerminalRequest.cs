using FluentTerminal.Models.Enums;
using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class CreateTerminalRequest : IMessage
    {
        public const byte Identifier = 0;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public TerminalSize Size { get; set; }

        [DataMember(Order = 2)]
        public ShellProfile Profile { get; set; }

        [DataMember(Order = 3)]
        public SessionType SessionType { get; set; }
    }
}