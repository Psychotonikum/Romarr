using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.PushBullet
{
    public class PushBulletException : RomarrException
    {
        public PushBulletException(string message)
            : base(message)
        {
        }

        public PushBulletException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
