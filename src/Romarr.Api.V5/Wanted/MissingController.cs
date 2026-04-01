using Microsoft.AspNetCore.Mvc;
using Romarr.Core.CustomFormats;
using Romarr.Core.Datastore;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Games;
using Romarr.SignalR;
using Romarr.Api.V5.Roms;
using Romarr.Http;
using Romarr.Http.Extensions;

namespace Romarr.Api.V5.Wanted;

[V5ApiController("wanted/missing")]
public class MissingController : RomControllerWithSignalR
{
    public MissingController(IRomService gameFileService,
                         IGameService seriesService,
                         IUpgradableSpecification upgradableSpecification,
                         ICustomFormatCalculationService formatCalculator,
                         IBroadcastSignalRMessage signalRBroadcaster)
        : base(gameFileService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
    {
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<RomResource> GetMissingGameFiles([FromQuery] PagingRequestResource paging, bool monitored = true, [FromQuery] MissingSubresource[]? includeSubresources = null)
    {
        var pagingResource = new PagingResource<RomResource>(paging);
        var pagingSpec = pagingResource.MapToPagingSpec<RomResource, Rom>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "roms.airDateUtc",
                "roms.lastSearchTime",
                "game.sortTitle"
            },
            "roms.airDateUtc",
            SortDirection.Ascending);

        if (monitored)
        {
            pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Game.Monitored == true);
        }
        else
        {
            pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Game.Monitored == false);
        }

        var includeSeries = includeSubresources.Contains(MissingSubresource.Game);
        var includeImages = includeSubresources.Contains(MissingSubresource.Images);

        var resource = pagingSpec.ApplyToPage(_romService.GameFilesWithoutFiles, v => MapToResource(v, includeSeries, false, includeImages));

        return resource;
    }
}
