using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class WriteDataRequest : IMessage
    {
        public const byte Identifier = 6;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public int TerminalId { get; set; }

        [DataMember(Order = 1)]
        public byte[] Data { get; set; }
    }
}