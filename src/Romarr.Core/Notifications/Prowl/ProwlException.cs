using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Prowl
{
    public class ProwlException : RomarrException
    {
        public ProwlException(string message)
            : base(message)
        {
        }

        public ProwlException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
