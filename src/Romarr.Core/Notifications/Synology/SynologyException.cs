using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Synology
{
    public class SynologyException : RomarrException
    {
        public SynologyException(string message)
            : base(message)
        {
        }

        public SynologyException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
