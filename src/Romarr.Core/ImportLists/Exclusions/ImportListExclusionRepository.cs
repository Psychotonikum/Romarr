using System.Linq;
using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        ImportListExclusion FindByIgdbId(int igdbId);
    }

    public class ImportListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public ImportListExclusion FindByIgdbId(int igdbId)
        {
            return Query(m => m.IgdbId == igdbId).SingleOrDefault();
        }
    }
}
