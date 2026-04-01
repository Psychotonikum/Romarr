using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;
using Romarr.Core.ThingiProvider.Status;

namespace Romarr.Core.Indexers
{
    public interface IIndexerStatusRepository : IProviderStatusRepository<IndexerStatus>
    {
    }

    public class IndexerStatusRepository : ProviderStatusRepository<IndexerStatus>, IIndexerStatusRepository
    {
        public IndexerStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
