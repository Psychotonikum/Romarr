using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.RemotePathMappings
{
    public interface IRemotePathMappingRepository : IBasicRepository<RemotePathMapping>
    {
    }

    public class RemotePathMappingRepository : BasicRepository<RemotePathMapping>, IRemotePathMappingRepository
    {
        public RemotePathMappingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;
    }
}
