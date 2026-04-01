using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.SendGrid
{
    public class SendGridException : RomarrException
    {
        public SendGridException(string message)
            : base(message)
        {
        }

        public SendGridException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
