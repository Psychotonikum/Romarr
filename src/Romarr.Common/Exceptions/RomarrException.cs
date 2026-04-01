using System;

namespace Romarr.Common.Exceptions
{
    public abstract class RomarrException : ApplicationException
    {
        protected RomarrException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        protected RomarrException(string message)
            : base(message)
        {
        }

        protected RomarrException(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }

        protected RomarrException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
