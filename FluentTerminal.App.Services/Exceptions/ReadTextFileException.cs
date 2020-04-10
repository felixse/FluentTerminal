using System;

namespace FluentTerminal.App.Services.Exceptions
{
    public class ReadTextFileException : Exception
    {
        public ReadTextFileException() : base()
        {
        }

        public ReadTextFileException(string message) : base(message)
        {
        }

        public ReadTextFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReadTextFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
