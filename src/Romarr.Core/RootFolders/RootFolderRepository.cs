using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {
    }

    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;
    }
}
