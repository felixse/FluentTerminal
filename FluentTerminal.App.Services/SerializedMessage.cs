using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.App.Services
{
    public class SerializedMessage
    {
        public SerializedMessage(byte identifier, byte[] data)
        {
            Identifier = identifier;
            Data = data;
        }

        public byte Identifier { get; }
        public byte[] Data { get; }
    }
}
