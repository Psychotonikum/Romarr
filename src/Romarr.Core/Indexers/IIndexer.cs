using System.Collections.Generic;
using System.Threading.Tasks;
using Romarr.Common.Http;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        DownloadProtocol Protocol { get; }

        Task<IList<ReleaseInfo>> FetchRecent();
        Task<IList<ReleaseInfo>> Fetch(PlatformSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(SingleGameFileSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(SpecialGameFileSearchCriteria searchCriteria);
        HttpRequest GetDownloadRequest(string link);
    }
}
