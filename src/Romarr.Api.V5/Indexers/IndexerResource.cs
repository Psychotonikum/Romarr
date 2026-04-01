using Romarr.Core.Indexers;
using Romarr.Api.V5.Provider;

namespace Romarr.Api.V5.Indexers;

public class IndexerResource : ProviderResource<IndexerResource>
{
    public bool EnableRss { get; set; }
    public bool EnableAutomaticSearch { get; set; }
    public bool EnableInteractiveSearch { get; set; }
    public bool SupportsRss { get; set; }
    public bool SupportsSearch { get; set; }
    public DownloadProtocol Protocol { get; set; }
    public int Priority { get; set; }
    public int PlatformSearchMaximumSingleFileAge { get; set; }
    public int DownloadClientId { get; set; }
}

public class IndexerResourceMapper : ProviderResourceMapper<IndexerResource, IndexerDefinition>
{
    public override IndexerResource ToResource(IndexerDefinition definition)
    {
        var resource = base.ToResource(definition);

        resource.EnableRss = definition.EnableRss;
        resource.EnableAutomaticSearch = definition.EnableAutomaticSearch;
        resource.EnableInteractiveSearch = definition.EnableInteractiveSearch;
        resource.SupportsRss = definition.SupportsRss;
        resource.SupportsSearch = definition.SupportsSearch;
        resource.Protocol = definition.Protocol;
        resource.Priority = definition.Priority;
        resource.PlatformSearchMaximumSingleFileAge = definition.PlatformSearchMaximumSingleFileAge;
        resource.DownloadClientId = definition.DownloadClientId;

        return resource;
    }

    public override IndexerDefinition ToModel(IndexerResource resource, IndexerDefinition? existingDefinition)
    {
        var definition = base.ToModel(resource, existingDefinition);

        definition.EnableRss = resource.EnableRss;
        definition.EnableAutomaticSearch = resource.EnableAutomaticSearch;
        definition.EnableInteractiveSearch = resource.EnableInteractiveSearch;
        definition.Priority = resource.Priority;
        definition.PlatformSearchMaximumSingleFileAge = resource.PlatformSearchMaximumSingleFileAge;
        definition.DownloadClientId = resource.DownloadClientId;

        return definition;
    }
}
