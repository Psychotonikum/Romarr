using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Romarr.Common.Extensions;
using Romarr.Core.CustomFormats;
using Romarr.Core.Datastore;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Download;
using Romarr.Core.History;
using Romarr.Core.Games;
using Romarr.Api.V3.Roms;
using Romarr.Api.V3.Game;
using Romarr.Http;
using Romarr.Http.Extensions;

namespace Romarr.Api.V3.History
{
    [V3ApiController]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly IGameService _gameService;

        public HistoryController(IHistoryService historyService,
                             ICustomFormatCalculationService formatCalculator,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService,
                             IGameService seriesService)
        {
            _historyService = historyService;
            _formatCalculator = formatCalculator;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
            _gameService = seriesService;
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
        public PagingResource<HistoryResource> GetHistory([FromQuery] PagingRequestResource paging, bool includeSeries, bool includeGameFile, [FromQuery(Name = "eventType")] int[] eventTypes, int? romId, string downloadId, [FromQuery] int[] gameIds = null, [FromQuery] int[] languages = null, [FromQuery] int[] quality = null)
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

            return pagingSpec.ApplyToPage(h => _historyService.Paged(pagingSpec, languages, quality), h => MapToResource(h, includeSeries, includeGameFile));
        }

        [HttpGet("since")]
        [Produces("application/json")]
        public List<HistoryResource> GetHistorySince(DateTime date, FileHistoryEventType? eventType = null, bool includeSeries = false, bool includeGameFile = false)
        {
            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeSeries, includeGameFile)).ToList();
        }

        [HttpGet("game")]
        [Produces("application/json")]
        public List<HistoryResource> GetSeriesHistory(int gameId, int? platformNumber, FileHistoryEventType? eventType = null, bool includeSeries = false, bool includeGameFile = false)
        {
            var game = _gameService.GetGame(gameId);

            if (platformNumber.HasValue)
            {
                return _historyService.GetByPlatform(gameId, platformNumber.Value, eventType).Select(h =>
                {
                    h.Game = game;

                    return MapToResource(h, includeSeries, includeGameFile);
                }).ToList();
            }

            return _historyService.GetBySeries(gameId, eventType).Select(h =>
            {
                h.Game = game;

                return MapToResource(h, includeSeries, includeGameFile);
            }).ToList();
        }

        [HttpPost("failed/{id}")]
        public object MarkAsFailed([FromRoute] int id)
        {
            _failedDownloadService.MarkAsFailed(id);
            return new { };
        }
    }
}
