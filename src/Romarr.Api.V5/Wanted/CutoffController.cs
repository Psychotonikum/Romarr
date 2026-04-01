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

[V5ApiController("wanted/cutoff")]
public class CutoffController : RomControllerWithSignalR
{
    private readonly IFileCutoffService _romCutoffService;

    public CutoffController(IFileCutoffService gameFileCutoffService,
                        IRomService gameFileService,
                        IGameService seriesService,
                        IUpgradableSpecification upgradableSpecification,
                        ICustomFormatCalculationService formatCalculator,
                        IBroadcastSignalRMessage signalRBroadcaster)
        : base(gameFileService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
    {
        _romCutoffService = gameFileCutoffService;
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<RomResource> GetCutoffUnmetGameFiles([FromQuery] PagingRequestResource paging, bool monitored = true, [FromQuery] CutoffSubresource[]? includeSubresources = null)
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

        var includeSeries = includeSubresources.Contains(CutoffSubresource.Game);
        var includeRomFile = includeSubresources.Contains(CutoffSubresource.RomFile);
        var includeImages = includeSubresources.Contains(CutoffSubresource.Images);

        var resource = pagingSpec.ApplyToPage(_romCutoffService.GameFilesWhereCutoffUnmet, v => MapToResource(v, includeSeries, includeRomFile, includeImages));

        return resource;
    }
}
