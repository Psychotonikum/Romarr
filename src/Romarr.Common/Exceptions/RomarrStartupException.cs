using System;

namespace Romarr.Common.Exceptions
{
    public class RomarrStartupException : RomarrException
    {
        public RomarrStartupException(string message, params object[] args)
            : base("Romarr failed to start: " + string.Format(message, args))
        {
        }

        public RomarrStartupException(string message)
            : base("Romarr failed to start: " + message)
        {
        }

        public RomarrStartupException()
            : base("Romarr failed to start")
        {
        }

        public RomarrStartupException(Exception innerException, string message, params object[] args)
            : base("Romarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public RomarrStartupException(Exception innerException, string message)
            : base("Romarr failed to start: " + message, innerException)
        {
        }

        public RomarrStartupException(Exception innerException)
            : base("Romarr failed to start: " + innerException.Message)
        {
        }
    }
}
