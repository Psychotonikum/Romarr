using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Discord
{
    public class DiscordException : RomarrException
    {
        public DiscordException(string message)
            : base(message)
        {
        }

        public DiscordException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
