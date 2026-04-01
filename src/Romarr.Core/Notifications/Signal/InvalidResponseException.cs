using System;

namespace Romarr.Core.Notifications.Signal
{
    public class SignalInvalidResponseException : Exception
    {
        public SignalInvalidResponseException()
        {
        }

        public SignalInvalidResponseException(string message)
            : base(message)
        {
        }
    }
}
