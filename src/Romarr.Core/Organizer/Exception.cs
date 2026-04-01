using Romarr.Common.Exceptions;

namespace Romarr.Core.Organizer
{
    public class NamingFormatException : RomarrException
    {
        public NamingFormatException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NamingFormatException(string message)
            : base(message)
        {
        }
    }
}
