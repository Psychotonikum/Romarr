using Microsoft.AspNetCore.Mvc;
using Romarr.Core.CustomFormats;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Games;
using Romarr.SignalR;
using Romarr.Http;
using Romarr.Http.REST;
using Romarr.Http.REST.Attributes;

namespace Romarr.Api.V5.Roms;

[V5ApiController]
public class RomController : RomControllerWithSignalR
{
    public RomController(IGameService seriesService,
                         IRomService gameFileService,
                         IUpgradableSpecification upgradableSpecification,
                         ICustomFormatCalculationService formatCalculator,
                         IBroadcastSignalRMessage signalRBroadcaster)
        : base(gameFileService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
    {
    }

    [HttpGet]
    [Produces("application/json")]
    public List<RomResource> GetRoms(int? gameId, int? platformNumber, [FromQuery]List<int> romIds, int? romFileId, [FromQuery] FileSubresource[]? includeSubresources = null)
    {
        var includeSeries = includeSubresources.Contains(FileSubresource.Game);
        var includeRomFile = includeSubresources.Contains(FileSubresource.RomFile);
        var includeImages = includeSubresources.Contains(FileSubresource.Images);

        if (gameId.HasValue)
        {
            if (platformNumber.HasValue)
            {
                return MapToResource(_romService.GetRomsByPlatform(gameId.Value, platformNumber.Value), includeSeries, includeRomFile, includeImages);
            }

            return MapToResource(_romService.GetGameFileBySeries(gameId.Value), includeSeries, includeRomFile, includeImages);
        }
        else if (romIds.Any())
        {
            return MapToResource(_romService.GetRoms(romIds), includeSeries, includeRomFile, includeImages);
        }
        else if (romFileId.HasValue)
        {
            return MapToResource(_romService.GetRomsByFileId(romFileId.Value), includeSeries, includeRomFile, includeImages);
        }

        throw new BadRequestException("gameId or romIds must be provided");
    }

    [RestPutById]
    [Consumes("application/json")]
    public ActionResult<RomResource> SetGameFileMonitored([FromRoute] int id, [FromBody] RomResource resource)
    {
        _romService.SetGameFileMonitored(id, resource.Monitored);

        resource = MapToResource(_romService.GetGameFile(id), false, false, false);

        return Accepted(resource);
    }

    [HttpPut("monitor")]
    [Consumes("application/json")]
    public IActionResult SetGameFilesMonitored([FromBody] FilesMonitoredResource resource, [FromQuery] FileSubresource[]? includeSubresources = null)
    {
        var includeImages = includeSubresources.Contains(FileSubresource.Images);

        if (resource.RomIds.Count == 1)
        {
            _romService.SetGameFileMonitored(resource.RomIds.First(), resource.Monitored);
        }
        else
        {
            _romService.SetMonitored(resource.RomIds, resource.Monitored);
        }

        var resources = MapToResource(_romService.GetRoms(resource.RomIds), false, false, includeImages);

        return Accepted(resources);
    }
}
