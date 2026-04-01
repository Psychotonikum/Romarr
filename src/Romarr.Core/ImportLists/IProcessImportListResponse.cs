using System.Collections.Generic;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.ImportLists
{
    public interface IParseImportListResponse
    {
        IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse);
    }
}
