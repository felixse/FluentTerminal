using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models.Requests
{
    public class CreateTerminalRequest : IMessage
    {
        public byte Identifier => (byte) MessageIdentifiers.CreateTerminalRequest;

        public byte Id { get; set; }
        public TerminalSize Size { get; set; }
        public ShellProfile Profile { get; set; }
        public SessionType SessionType { get; set; }
    }
}