using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.App.Services.Exceptions
{
    public class SaveTextFileException : Exception
    {
        public SaveTextFileException() : base()
        {
        }

        public SaveTextFileException(string message) : base(message)
        {
        }

        public SaveTextFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SaveTextFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
