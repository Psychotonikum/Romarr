using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Ntfy
{
    public class NtfyException : RomarrException
    {
        public NtfyException(string message)
            : base(message)
        {
        }

        public NtfyException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
