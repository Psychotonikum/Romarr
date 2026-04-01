using Romarr.Common.Exceptions;

namespace Romarr.Core.Parser
{
    public class InvalidPlatformException : RomarrException
    {
        public InvalidPlatformException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidPlatformException(string message)
            : base(message)
        {
        }
    }
}
