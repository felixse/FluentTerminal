using System.Runtime.Serialization;

namespace FluentTerminal.Models.Responses
{
    [DataContract]
    public class GetAvailablePortResponse : IMessage
    {
        public const byte Identifier = 8;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public int Port { get; set; }
    }
}
