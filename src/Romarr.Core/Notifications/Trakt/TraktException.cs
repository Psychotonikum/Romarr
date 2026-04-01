using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Trakt
{
    public class TraktException : RomarrException
    {
        public TraktException(string message)
            : base(message)
        {
        }

        public TraktException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
