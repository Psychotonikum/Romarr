using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Download
{
    public interface IDownloadClientRepository : IProviderRepository<DownloadClientDefinition>
    {
    }

    public class DownloadClientRepository : ProviderRepository<DownloadClientDefinition>, IDownloadClientRepository
    {
        public DownloadClientRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
