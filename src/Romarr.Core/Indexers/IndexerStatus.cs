using Romarr.Core.Parser.Model;
using Romarr.Core.ThingiProvider.Status;

namespace Romarr.Core.Indexers
{
    public class IndexerStatus : ProviderStatusBase
    {
        public ReleaseInfo LastRssSyncReleaseInfo { get; set; }
    }
}
