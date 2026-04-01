using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Romarr.Common.Cache;
using Romarr.Common.EnsureThat;
using Romarr.Common.Extensions;
using Romarr.Core.DecisionEngine;
using Romarr.Core.Download;
using Romarr.Core.Exceptions;
using Romarr.Core.Indexers;
using Romarr.Core.IndexerSearch;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Games;
using Romarr.Core.Validation;
using Romarr.Http;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Romarr.Api.V3.Indexers
{
    [V3ApiController]
    public class ReleaseController : ReleaseControllerBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly IGameService _gameService;
        private readonly IRomService _romService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        private readonly ICached<RemoteRom> _remoteRomCache;

        public ReleaseController(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForReleases releaseSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             IGameService seriesService,
                             IRomService gameFileService,
                             IParsingService parsingService,
                             ICacheManager cacheManager,
                             IQualityProfileService qualityProfileService,
                             Logger logger)
            : base(qualityProfileService)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _releaseSearchService = releaseSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _gameService = seriesService;
            _romService = gameFileService;
            _parsingService = parsingService;
            _logger = logger;

            PostValidator.RuleFor(s => s.IndexerId).ValidId();
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteRomCache = cacheManager.GetCache<RemoteRom>(GetType(), "remoteRoms");
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<object> DownloadRelease([FromBody] ReleaseResource release)
        {
            var remoteRom = _remoteRomCache.Find(GetCacheKey(release));

            if (remoteRom == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new RomarrClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                if (release.ShouldOverride == true)
                {
                    Ensure.That(release.GameId, () => release.GameId).IsNotNull();
                    Ensure.That(release.RomIds, () => release.RomIds).IsNotNull();
                    Ensure.That(release.RomIds, () => release.RomIds).HasItems();
                    Ensure.That(release.Quality, () => release.Quality).IsNotNull();
                    Ensure.That(release.Languages, () => release.Languages).IsNotNull();

                    // Clone the remote rom so we don't overwrite anything on the original
                    remoteRom = new RemoteRom
                    {
                        Release = remoteRom.Release,
                        ParsedRomInfo = remoteRom.ParsedRomInfo.JsonClone(),
                        SceneMapping = remoteRom.SceneMapping,
                        MappedPlatformNumber = remoteRom.MappedPlatformNumber,
                        GameFileRequested = remoteRom.GameFileRequested,
                        DownloadAllowed = remoteRom.DownloadAllowed,
                        SeedConfiguration = remoteRom.SeedConfiguration,
                        CustomFormats = remoteRom.CustomFormats,
                        CustomFormatScore = remoteRom.CustomFormatScore,
                        SeriesMatchType = remoteRom.SeriesMatchType,
                        ReleaseSource = remoteRom.ReleaseSource
                    };

                    remoteRom.Game = _gameService.GetGame(release.GameId!.Value);
                    remoteRom.Roms = _romService.GetRoms(release.RomIds);
                    remoteRom.ParsedRomInfo.Quality = release.Quality;
                    remoteRom.Languages = release.Languages;
                }

                if (remoteRom.Game == null)
                {
                    if (release.FileId.HasValue)
                    {
                        var rom = _romService.GetGameFile(release.FileId.Value);

                        remoteRom.Game = _gameService.GetGame(rom.GameId);
                        remoteRom.Roms = new List<Rom> { rom };
                    }
                    else if (release.GameId.HasValue)
                    {
                        var game = _gameService.GetGame(release.GameId.Value);
                        var roms = _parsingService.GetRoms(remoteRom.ParsedRomInfo, game, true);

                        if (roms.Empty())
                        {
                            throw new RomarrClientException(HttpStatusCode.NotFound, "Unable to parse roms in the release, will need to be manually provided");
                        }

                        remoteRom.Game = game;
                        remoteRom.Roms = roms;
                    }
                    else
                    {
                        throw new RomarrClientException(HttpStatusCode.NotFound, "Unable to find matching game and roms, will need to be manually provided");
                    }
                }
                else if (remoteRom.Roms.Empty())
                {
                    var roms = _parsingService.GetRoms(remoteRom.ParsedRomInfo, remoteRom.Game, true);

                    if (roms.Empty() && release.FileId.HasValue)
                    {
                        var rom = _romService.GetGameFile(release.FileId.Value);

                        roms = new List<Rom> { rom };
                    }

                    remoteRom.Roms = roms;
                }

                if (remoteRom.Roms.Empty())
                {
                    throw new RomarrClientException(HttpStatusCode.NotFound, "Unable to parse roms in the release, will need to be manually provided");
                }

                await _downloadService.DownloadReport(remoteRom, release.DownloadClientId);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex, ex.Message);
                throw new RomarrClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<List<ReleaseResource>> GetReleases(int? gameId, int? romId, int? platformNumber)
        {
            if (romId.HasValue)
            {
                return await GetGameFileReleases(romId.Value);
            }

            if (gameId.HasValue && platformNumber.HasValue)
            {
                return await GetPlatformReleases(gameId.Value, platformNumber.Value);
            }

            return await GetRss();
        }

        private async Task<List<ReleaseResource>> GetGameFileReleases(int romId)
        {
            try
            {
                var decisions = await _releaseSearchService.RomSearch(romId, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (SearchFailedException ex)
            {
                throw new RomarrClientException(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Rom search failed: " + ex.Message);
                throw new RomarrClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task<List<ReleaseResource>> GetPlatformReleases(int gameId, int platformNumber)
        {
            try
            {
                var decisions = await _releaseSearchService.PlatformSearch(gameId, platformNumber, false, false, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (SearchFailedException ex)
            {
                throw new RomarrClientException(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Platform search failed: " + ex.Message);
                throw new RomarrClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task<List<ReleaseResource>> GetRss()
        {
            var reports = await _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

            return MapDecisions(prioritizedDecisions);
        }

        protected override ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var resource = base.MapDecision(decision, initialWeight);
            _remoteRomCache.Set(GetCacheKey(resource), decision.RemoteRom, TimeSpan.FromMinutes(30));

            return resource;
        }

        private string GetCacheKey(ReleaseResource resource)
        {
            return string.Concat(resource.IndexerId, "_", resource.Guid);
        }
    }
}
