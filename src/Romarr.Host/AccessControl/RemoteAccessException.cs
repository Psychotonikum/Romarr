using System;
using Romarr.Common.Exceptions;

namespace Romarr.Host.AccessControl
{
    public class RemoteAccessException : RomarrException
    {
        public RemoteAccessException(string message, params object[] args)
            : base(message, args)
        {
        }

        public RemoteAccessException(string message)
            : base(message)
        {
        }

        public RemoteAccessException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public RemoteAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
