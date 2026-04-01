using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Extras.Metadata
{
    public interface IMetadataRepository : IProviderRepository<MetadataDefinition>
    {
    }

    public class MetadataRepository : ProviderRepository<MetadataDefinition>, IMetadataRepository
    {
        public MetadataRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
