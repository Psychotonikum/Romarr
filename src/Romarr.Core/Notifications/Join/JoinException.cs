using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Join
{
    public class JoinException : RomarrException
    {
        public JoinException(string message)
            : base(message)
        {
        }

        public JoinException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
