using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Apprise
{
    public class AppriseException : RomarrException
    {
        public AppriseException(string message)
            : base(message)
        {
        }

        public AppriseException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
