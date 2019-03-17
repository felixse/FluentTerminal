using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class TerminalExitedRequest : IMessage
    {
        public const byte Identifier = 5;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public int TerminalId { get; set; }
    }
}