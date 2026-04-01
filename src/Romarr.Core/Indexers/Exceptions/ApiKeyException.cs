using Romarr.Common.Exceptions;

namespace Romarr.Core.Indexers.Exceptions
{
    public class ApiKeyException : RomarrException
    {
        public ApiKeyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ApiKeyException(string message)
            : base(message)
        {
        }
    }
}
