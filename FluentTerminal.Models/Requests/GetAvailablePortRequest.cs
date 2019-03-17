using System.Runtime.Serialization;

namespace FluentTerminal.Models.Requests
{
    [DataContract]
    public class GetAvailablePortRequest : IMessage
    {
        public const byte Identifier = 2;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;
    }
}
