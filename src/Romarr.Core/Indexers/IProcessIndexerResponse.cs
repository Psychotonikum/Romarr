using System.Collections.Generic;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Indexers
{
    public interface IParseIndexerResponse
    {
        IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse);
    }
}
