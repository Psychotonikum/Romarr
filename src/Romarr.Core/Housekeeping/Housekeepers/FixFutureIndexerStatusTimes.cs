using Romarr.Core.Indexers;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class FixFutureIndexerStatusTimes : FixFutureProviderStatusTimes<IndexerStatus>, IHousekeepingTask
    {
        public FixFutureIndexerStatusTimes(IIndexerStatusRepository indexerStatusRepository)
            : base(indexerStatusRepository)
        {
        }
    }
}
