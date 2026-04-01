using Romarr.Core.Datastore;

namespace Romarr.Core.ImportLists.Exclusions
{
    public class ImportListExclusion : ModelBase
    {
        public int IgdbId { get; set; }
        public string Title { get; set; }
    }
}
