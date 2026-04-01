using Romarr.Core.Configuration;
using Romarr.Core.ImportLists;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Config
{
    public class ImportListConfigResource : RestResource
    {
        public ListSyncLevelType ListSyncLevel { get; set; }
        public int ListSyncTag { get; set; }
    }

    public static class ImportListConfigResourceMapper
    {
        public static ImportListConfigResource ToResource(IConfigService model)
        {
            return new ImportListConfigResource
            {
                ListSyncLevel = model.ListSyncLevel,
                ListSyncTag = model.ListSyncTag,
            };
        }
    }
}
