using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Plex
{
    public class PlexVersionException : RomarrException
    {
        public PlexVersionException(string message)
            : base(message)
        {
        }

        public PlexVersionException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
