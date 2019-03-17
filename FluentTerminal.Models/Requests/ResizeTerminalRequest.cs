using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class ResizeTerminalRequest : IMessage
    {
        public const byte Identifier = 3;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public int TerminalId { get; set; }

        [DataMember(Order = 1)]
        public TerminalSize NewSize { get; set; }
    }
}