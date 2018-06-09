using System;

namespace FluentTerminal.App.Exceptions
{
    public class ParseThemeException : Exception
    {
        public ParseThemeException() : base()
        {
        }

        public ParseThemeException(string message) : base(message)
        {
        }

        public ParseThemeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParseThemeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}