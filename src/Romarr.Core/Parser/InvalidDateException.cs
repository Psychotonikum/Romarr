using Romarr.Common.Exceptions;

namespace Romarr.Core.Parser
{
    public class InvalidDateException : RomarrException
    {
        public InvalidDateException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidDateException(string message)
            : base(message)
        {
        }
    }
}
