using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class DisplayTerminalOutputRequest : IMessage
    {
        public const byte Identifier = 1;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public int TerminalId { get; set; }

        [DataMember(Order = 1)]
        public byte[] Output { get; set; }
    }
}