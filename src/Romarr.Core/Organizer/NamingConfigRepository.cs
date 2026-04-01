using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.Organizer
{
    public interface INamingConfigRepository : IBasicRepository<NamingConfig>
    {
    }

    public class NamingConfigRepository : BasicRepository<NamingConfig>, INamingConfigRepository
    {
        public NamingConfigRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
