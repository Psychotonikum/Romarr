using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.CustomFilters
{
    public interface ICustomFilterRepository : IBasicRepository<CustomFilter>
    {
    }

    public class CustomFilterRepository : BasicRepository<CustomFilter>, ICustomFilterRepository
    {
        public CustomFilterRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
