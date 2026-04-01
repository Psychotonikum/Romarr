using Microsoft.AspNetCore.Mvc;
using Romarr.Common.Extensions;
using Romarr.Core.CustomFormats;
using Romarr.Core.Datastore;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Download;
using Romarr.Core.History;
using Romarr.Core.Games;
using Romarr.Api.V5.Roms;
using Romarr.Api.V5.Game;
using Romarr.Http;
using Romarr.Http.Extensions;

namespace Romarr.Api.V5.History;

[V5ApiController]
public class HistoryController : Controller
{
    private readonly IHistoryService _historyService;
    private readonly ICustomFormatCalculationService _formatCalculator;
    private readonly IUpgradableSpecification _upgradableSpecification;
    private readonly IFailedDownloadService _failedDownloadService;
    private readonly IGameService _gameService;
    private readonly IRomService _romService;

    public HistoryController(IHistoryService historyService,
                         ICustomFormatCalculationService formatCalculator,
                         IUpgradableSpecification upgradableSpecification,
                         IFailedDownloadService failedDownloadService,
                         IGameService seriesService,
                         IRomService gameFileService)
    {
        _historyService = historyService;
        _formatCalculator = formatCalculator;
        _upgradableSpecification = upgradableSpecification;
        _failedDownloadService = failedDownloadService;
        _gameService = seriesService;
        _romService = gameFileService;
    }

    protected HistoryResource MapToResource(FileHistory model, bool includeSeries, bool includeGameFile)
    {
        var resource = model.ToResource(_formatCalculator);

        if (includeSeries)
        {
            resource.Game = model.Game.ToResource();
        }

        if (includeGameFile)
        {
            resource.Rom = model.Rom.ToResource();
        }

        if (model.Game != null)
        {
            resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Game.QualityProfile.Value, model.Quality);
        }

        return resource;
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<HistoryResource> GetHistory([FromQuery] PagingRequestResource paging, [FromQuery(Name = "eventType")] int[]? eventTypes, int? romId, string? downloadId, [FromQuery] int[]? gameIds = null, [FromQuery] int[]? languages = null, [FromQuery] int[]? quality = null, [FromQuery] HistorySubresource[]? includeSubresources = null)
    {
        var pagingResource = new PagingResource<HistoryResource>(paging);
        var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, FileHistory>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "date",
                "game.sortTitle"
            },
            "date",
            SortDirection.Descending);

        if (eventTypes != null && eventTypes.Any())
        {
            pagingSpec.FilterExpressions.Add(v => eventTypes.Contains((int)v.EventType));
        }

        if (romId.HasValue)
        {
            pagingSpec.FilterExpressions.Add(h => h.FileId == romId);
        }

        if (downloadId.IsNotNullOrWhiteSpace())
        {
            pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
        }

        if (gameIds != null && gameIds.Any())
        {
            pagingSpec.FilterExpressions.Add(h => gameIds.Contains(h.GameId));
        }

        var includeSeries = includeSubresources.Contains(HistorySubresource.Game);
        var includeGameFile = includeSubresources.Contains(HistorySubresource.Rom);

        return pagingSpec.ApplyToPage(h => _historyService.Paged(pagingSpec, languages, quality), h => MapToResource(h, includeSeries, includeGameFile));
    }

    [HttpGet("since")]
    [Produces("application/json")]
    public List<HistoryResource> GetHistorySince(DateTime date, FileHistoryEventType? eventType = null, [FromQuery] HistorySubresource[]? includeSubresources = null)
    {
        var includeSeries = includeSubresources.Contains(HistorySubresource.Game);
        var includeGameFile = includeSubresources.Contains(HistorySubresource.Rom);

        return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeSeries, includeGameFile)).ToList();
    }

    [HttpGet("game")]
    [Produces("application/json")]
    public List<HistoryResource> GetSeriesHistory(int gameId, FileHistoryEventType? eventType = null, [FromQuery] HistorySubresource[]? includeSubresources = null)
    {
        var game = _gameService.GetGame(gameId);
        var includeSeries = includeSubresources.Contains(HistorySubresource.Game);
        var includeGameFile = includeSubresources.Contains(HistorySubresource.Rom);

        return _historyService.GetBySeries(gameId, eventType).Select(h =>
        {
            h.Game = game;

            return MapToResource(h, includeSeries, includeGameFile);
        }).ToList();
    }

    [HttpGet("platform")]
    [Produces("application/json")]
    public List<HistoryResource> GetPlatformHistory(int gameId, int platformNumber, FileHistoryEventType? eventType = null, [FromQuery] HistorySubresource[]? includeSubresources = null)
    {
        var game = _gameService.GetGame(gameId);
        var includeSeries = includeSubresources.Contains(HistorySubresource.Game);
        var includeGameFile = includeSubresources.Contains(HistorySubresource.Rom);

        return _historyService.GetByPlatform(gameId, platformNumber, eventType).Select(h =>
        {
            h.Game = game;

            return MapToResource(h, includeSeries, includeGameFile);
        }).ToList();
    }

    [HttpGet("rom")]
    [Produces("application/json")]
    public List<HistoryResource> GetFileHistory(int romId, FileHistoryEventType? eventType = null, [FromQuery] HistorySubresource[]? includeSubresources = null)
    {
        var rom = _romService.GetGameFile(romId);
        var game = _gameService.GetGame(rom.GameId);
        var includeSeries = includeSubresources.Contains(HistorySubresource.Game);
        var includeGameFile = includeSubresources.Contains(HistorySubresource.Rom);

        return _historyService.GetByGameFile(romId, eventType)
            .Select(h =>
            {
                h.Game = game;
                h.Rom = rom;

                return MapToResource(h, includeSeries, includeGameFile);
            }).ToList();
    }

    [HttpPost("failed/{id}")]
    public ActionResult MarkAsFailed([FromRoute] int id)
    {
        _failedDownloadService.MarkAsFailed(id);
        return NoContent();
    }
}
