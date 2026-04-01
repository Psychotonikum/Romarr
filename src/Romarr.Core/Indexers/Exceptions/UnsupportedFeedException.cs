using Romarr.Common.Exceptions;

namespace Romarr.Core.Indexers.Exceptions
{
    public class UnsupportedFeedException : RomarrException
    {
        public UnsupportedFeedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UnsupportedFeedException(string message)
            : base(message)
        {
        }
    }
}
