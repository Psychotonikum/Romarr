using Romarr.Core.Datastore;
using Romarr.Core.Extras.Files;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.Extras.Others
{
    public interface IOtherExtraFileRepository : IExtraFileRepository<OtherExtraFile>
    {
    }

    public class OtherExtraFileRepository : ExtraFileRepository<OtherExtraFile>, IOtherExtraFileRepository
    {
        public OtherExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
