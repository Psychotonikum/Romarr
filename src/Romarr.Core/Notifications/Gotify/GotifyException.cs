using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Gotify
{
    public class GotifyException : RomarrException
    {
        public GotifyException(string message)
            : base(message)
        {
        }

        public GotifyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public GotifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
