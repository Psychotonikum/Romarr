using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;
using Romarr.Core.ThingiProvider.Status;

namespace Romarr.Core.ImportLists
{
    public interface IImportListStatusRepository : IProviderStatusRepository<ImportListStatus>
    {
    }

    public class ImportListStatusRepository : ProviderStatusRepository<ImportListStatus>, IImportListStatusRepository
    {
        public ImportListStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
