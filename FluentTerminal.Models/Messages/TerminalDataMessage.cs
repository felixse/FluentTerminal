namespace FluentTerminal.Models.Messages
{
    public class TerminalDataMessage
    {
        public byte TerminalId { get; }

        public byte[] Data { get; }

        public TerminalDataMessage(byte terminalId, byte[] data)
        {
            TerminalId = terminalId;
            Data = data;
        }
    }
}
