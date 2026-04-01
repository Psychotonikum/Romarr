using Romarr.Common.Exceptions;

namespace Romarr.Core.Exceptions
{
    public class SearchFailedException : RomarrException
    {
        public SearchFailedException(string message)
            : base(message)
        {
        }
    }
}
