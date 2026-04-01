using System.Collections.Generic;
using Romarr.Core.Indexers;

namespace Romarr.Api.V3.Indexers
{
    public class IndexerBulkResource : ProviderBulkResource<IndexerBulkResource>
    {
        public bool? EnableRss { get; set; }
        public bool? EnableAutomaticSearch { get; set; }
        public bool? EnableInteractiveSearch { get; set; }
        public int? Priority { get; set; }
        public int? PlatformSearchMaximumSingleFileAge { get; set; }
    }

    public class IndexerBulkResourceMapper : ProviderBulkResourceMapper<IndexerBulkResource, IndexerDefinition>
    {
        public override List<IndexerDefinition> UpdateModel(IndexerBulkResource resource, List<IndexerDefinition> existingDefinitions)
        {
            if (resource == null)
            {
                return new List<IndexerDefinition>();
            }

            existingDefinitions.ForEach(existing =>
            {
                existing.EnableRss = resource.EnableRss ?? existing.EnableRss;
                existing.EnableAutomaticSearch = resource.EnableAutomaticSearch ?? existing.EnableAutomaticSearch;
                existing.EnableInteractiveSearch = resource.EnableInteractiveSearch ?? existing.EnableInteractiveSearch;
                existing.Priority = resource.Priority ?? existing.Priority;
                existing.PlatformSearchMaximumSingleFileAge = resource.PlatformSearchMaximumSingleFileAge ?? existing.PlatformSearchMaximumSingleFileAge;
            });

            return existingDefinitions;
        }
    }
}
