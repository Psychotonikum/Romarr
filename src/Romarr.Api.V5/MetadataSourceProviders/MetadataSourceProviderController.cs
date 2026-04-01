using Romarr.Core.MetadataSource.Providers;
using Romarr.SignalR;
using Romarr.Api.V5.Provider;
using Romarr.Http;

namespace Romarr.Api.V5.MetadataSourceProviders;

[V5ApiController]
public class MetadataSourceProviderController : ProviderControllerBase<MetadataSourceProviderResource, MetadataSourceProviderBulkResource, IMetadataSourceProvider, MetadataSourceDefinition>
{
    public static readonly MetadataSourceProviderResourceMapper ResourceMapper = new();
    public static readonly MetadataSourceProviderBulkResourceMapper BulkResourceMapper = new();

    public MetadataSourceProviderController(IBroadcastSignalRMessage signalRBroadcaster,
        MetadataSourceProviderFactory metadataSourceProviderFactory)
        : base(signalRBroadcaster, metadataSourceProviderFactory, "metadatasourceprovider", ResourceMapper, BulkResourceMapper)
    {
    }
}
