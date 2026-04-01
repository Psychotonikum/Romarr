using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Blocklisting;
using Romarr.Core.CustomFormats;
using Romarr.Core.Datastore;
using Romarr.Core.Indexers;
using Romarr.Http;
using Romarr.Http.Extensions;
using Romarr.Http.REST.Attributes;

namespace Romarr.Api.V5.Blocklist;

[V5ApiController]
public class BlocklistController : Controller
{
    private readonly IBlocklistService _blocklistService;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public BlocklistController(IBlocklistService blocklistService,
                               ICustomFormatCalculationService formatCalculator)
    {
        _blocklistService = blocklistService;
        _formatCalculator = formatCalculator;
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<BlocklistResource> GetBlocklist([FromQuery] PagingRequestResource paging, [FromQuery] int[]? gameIds = null, [FromQuery] DownloadProtocol[]? protocols = null)
    {
        var pagingResource = new PagingResource<BlocklistResource>(paging);
        var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, Romarr.Core.Blocklisting.Blocklist>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "date",
                "indexer",
                "game.sortTitle",
                "sourceTitle"
            },
            "date",
            SortDirection.Descending);

        if (gameIds?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(b => gameIds.Contains(b.GameId));
        }

        if (protocols?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(b => protocols.Contains(b.Protocol));
        }

        return pagingSpec.ApplyToPage(b => _blocklistService.Paged(pagingSpec), b => BlocklistResourceMapper.MapToResource(b, _formatCalculator));
    }

    [RestDeleteById]
    public ActionResult DeleteBlocklist(int id)
    {
        _blocklistService.Delete(id);

        return NoContent();
    }

    [HttpDelete("bulk")]
    [Produces("application/json")]
    public ActionResult Remove([FromBody] BlocklistBulkResource resource)
    {
        _blocklistService.Delete(resource.Ids);

        return NoContent();
    }
}
