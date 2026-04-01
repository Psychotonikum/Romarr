using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.MetadataSource.Providers
{
    public interface IMetadataSourceProviderRepository : IProviderRepository<MetadataSourceDefinition>
    {
    }

    public class MetadataSourceProviderRepository : ProviderRepository<MetadataSourceDefinition>, IMetadataSourceProviderRepository
    {
        public MetadataSourceProviderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
