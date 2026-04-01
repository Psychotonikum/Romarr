using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.Profiles.Releases
{
    public interface IRestrictionRepository : IBasicRepository<ReleaseProfile>
    {
    }

    public class ReleaseProfileRepository : BasicRepository<ReleaseProfile>, IRestrictionRepository
    {
        public ReleaseProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
