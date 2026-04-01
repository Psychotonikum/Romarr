using System.Collections.Generic;
using Romarr.Common.Http;
using Romarr.Core.IndexerSearch.Definitions;

namespace Romarr.Core.Indexers.Torrentleech
{
    public class TorrentleechRequestGenerator : IIndexerRequestGenerator
    {
        public TorrentleechSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRssRequests(null));

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

        private IEnumerable<IndexerRequest> GetRssRequests(string searchParameters)
        {
            yield return new IndexerRequest(string.Format("{0}/{1}{2}", Settings.BaseUrl.Trim().TrimEnd('/'), Settings.ApiKey, searchParameters), HttpAccept.Rss);
        }
    }
}
