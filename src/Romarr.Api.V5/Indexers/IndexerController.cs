using FluentValidation;
using Romarr.Core.Indexers;
using Romarr.Core.Validation;
using Romarr.SignalR;
using Romarr.Api.V5.Provider;
using Romarr.Http;

namespace Romarr.Api.V5.Indexers;

[V5ApiController]
public class IndexerController : ProviderControllerBase<IndexerResource, IndexerBulkResource, IIndexer, IndexerDefinition>
{
    public static readonly IndexerResourceMapper ResourceMapper = new();
    public static readonly IndexerBulkResourceMapper BulkResourceMapper = new();

    public IndexerController(IBroadcastSignalRMessage signalRBroadcaster,
        IndexerFactory indexerFactory,
        DownloadClientExistsValidator downloadClientExistsValidator)
        : base(signalRBroadcaster, indexerFactory, "indexer", ResourceMapper, BulkResourceMapper)
    {
        SharedValidator.RuleFor(c => c.Priority).InclusiveBetween(1, 50);
        SharedValidator.RuleFor(c => c.PlatformSearchMaximumSingleFileAge).GreaterThanOrEqualTo(0);
        SharedValidator.RuleFor(c => c.DownloadClientId).SetValidator(downloadClientExistsValidator);
    }
}
