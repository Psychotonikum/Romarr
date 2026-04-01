using Romarr.Common.Exceptions;

namespace Romarr.Common.Disk
{
    public class NotParentException : RomarrException
    {
        public NotParentException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NotParentException(string message)
            : base(message)
        {
        }
    }
}
