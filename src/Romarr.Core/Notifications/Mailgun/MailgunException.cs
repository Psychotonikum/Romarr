using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Mailgun
{
    public class MailgunException : RomarrException
    {
        public MailgunException(string message)
            : base(message)
        {
        }

        public MailgunException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
