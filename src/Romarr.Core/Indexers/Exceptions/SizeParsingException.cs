using Romarr.Common.Exceptions;

namespace Romarr.Core.Indexers.Exceptions
{
    public class SizeParsingException : RomarrException
    {
        public SizeParsingException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
