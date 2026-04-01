using Romarr.Common.Http;
using Romarr.Core.IndexerSearch.Definitions;

namespace Romarr.Core.Indexers
{
    public class RssIndexerRequestGenerator : IIndexerRequestGenerator
    {
        private readonly string _baseUrl;

        public RssIndexerRequestGenerator(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(new[] { new IndexerRequest(_baseUrl, HttpAccept.Rss) });

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleGameFileSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(PlatformSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialGameFileSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }
    }
}
