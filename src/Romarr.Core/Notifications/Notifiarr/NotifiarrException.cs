using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Notifiarr
{
    public class NotifiarrException : RomarrException
    {
        public NotifiarrException(string message)
            : base(message)
        {
        }

        public NotifiarrException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
