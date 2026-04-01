using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Twitter
{
    public class TwitterException : RomarrException
    {
        public TwitterException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TwitterException(string message)
            : base(message)
        {
        }

        public TwitterException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public TwitterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
