using Romarr.Core.Indexers.Exceptions;

namespace Romarr.Core.Indexers.Newznab
{
    public class NewznabException : IndexerException
    {
        public NewznabException(IndexerResponse response, string message, params object[] args)
            : base(response, message, args)
        {
        }

        public NewznabException(IndexerResponse response, string message)
            : base(response, message)
        {
        }
    }
}
