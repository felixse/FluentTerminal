using System.Runtime.Serialization;

namespace FluentTerminal.Models.Responses
{
    [DataContract]
    public class CreateTerminalResponse : IMessage
    {
        public const byte Identifier = 7;

        [IgnoreDataMember]
        byte IMessage.Identifier => Identifier;

        [DataMember(Order = 0)]
        public bool Success { get; set; }

        [DataMember(Order = 1)]
        public string Error { get; set; }

        [DataMember(Order = 2)]
        public string ShellExecutableName { get; set; }
    }
}