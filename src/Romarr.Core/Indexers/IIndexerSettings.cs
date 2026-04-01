using System.Collections.Generic;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }

        IEnumerable<int> MultiLanguages { get; set; }

        IEnumerable<int> FailDownloads { get; set; }
    }
}
