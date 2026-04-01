using Romarr.Core.IndexerSearch.Definitions;

namespace Romarr.Core.Indexers
{
    public interface IIndexerRequestGenerator
    {
        IndexerPageableRequestChain GetRecentRequests();
        IndexerPageableRequestChain GetSearchRequests(SingleGameFileSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(PlatformSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(SpecialGameFileSearchCriteria searchCriteria);
    }
}
