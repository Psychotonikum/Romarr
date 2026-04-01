using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookException : RomarrException
    {
        public WebhookException(string message)
            : base(message)
        {
        }

        public WebhookException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
