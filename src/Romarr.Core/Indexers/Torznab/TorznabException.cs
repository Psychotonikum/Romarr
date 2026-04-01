using Romarr.Common.Exceptions;

namespace Romarr.Core.Indexers.Torznab
{
    public class TorznabException : RomarrException
    {
        public TorznabException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TorznabException(string message)
            : base(message)
        {
        }
    }
}
