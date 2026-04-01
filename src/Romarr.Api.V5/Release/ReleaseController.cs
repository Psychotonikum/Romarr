using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Romarr.Common.Cache;
using Romarr.Common.EnsureThat;
using Romarr.Common.Extensions;
using Romarr.Core.DecisionEngine;
using Romarr.Core.Download;
using Romarr.Core.Exceptions;
using Romarr.Core.History;
using Romarr.Core.Indexers;
using Romarr.Core.IndexerSearch;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Games;
using Romarr.Core.Validation;
using Romarr.Http;
using Romarr.Http.REST;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Romarr.Api.V5.Release;

[V5ApiController]
public class ReleaseController : RestController<ReleaseResource>
{
    private readonly IFetchAndParseRss _rssFetcherAndParser;
    private readonly ISearchForReleases _releaseSearchService;
    private readonly IMakeDownloadDecision _downloadDecisionMaker;
    private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
    private readonly IDownloadService _downloadService;
    private readonly IGameService _gameService;
    private readonly IRomService _romService;
    private readonly IParsingService _parsingService;
    private readonly IHistoryService _historyService;
    private readonly Logger _logger;

    private readonly QualityProfile _qualityProfile;
    private readonly ICached<RemoteRom> _remoteRomCache;

    public ReleaseController(IFetchAndParseRss rssFetcherAndParser,
                         ISearchForReleases releaseSearchService,
                         IMakeDownloadDecision downloadDecisionMaker,
                         IPrioritizeDownloadDecision prioritizeDownloadDecision,
                         IDownloadService downloadService,
                         IGameService seriesService,
                         IRomService gameFileService,
                         IParsingService parsingService,
                         IHistoryService historyService,
                         ICacheManager cacheManager,
                         IQualityProfileService qualityProfileService,
                         Logger logger)
    {
        _rssFetcherAndParser = rssFetcherAndParser;
        _releaseSearchService = releaseSearchService;
        _downloadDecisionMaker = downloadDecisionMaker;
        _prioritizeDownloadDecision = prioritizeDownloadDecision;
        _downloadService = downloadService;
        _gameService = seriesService;
        _romService = gameFileService;
        _parsingService = parsingService;
        _historyService = historyService;
        _logger = logger;

        _qualityProfile = qualityProfileService.GetDefaultProfile(string.Empty);
        _remoteRomCache = cacheManager.GetCache<RemoteRom>(GetType(), "remoteRoms");

        PostValidator.RuleFor(s => s.Release).NotNull();
        PostValidator.RuleFor(s => s.Release!.IndexerId).ValidId();
        PostValidator.RuleFor(s => s.Release!.Guid).NotEmpty();
    }

    [NonAction]
    public override ActionResult<ReleaseResource> GetResourceByIdWithErrorHandler(int id)
    {
        return base.GetResourceByIdWithErrorHandler(id);
    }

    protected override ReleaseResource GetResourceById(int id)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<object> DownloadRelease([FromBody] ReleaseGrabResource release)
    {
        var remoteRom = _remoteRomCache.Find(GetCacheKey(release));

        if (remoteRom == null)
        {
            _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

            throw new RomarrClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
        }

        try
        {
            if (release.Override != null)
            {
                var overrideInfo = release.Override;

                Ensure.That(overrideInfo.GameId, () => release.Override.GameId).IsNotNull();
                Ensure.That(overrideInfo.RomIds, () => overrideInfo.RomIds).IsNotNull();
                Ensure.That(overrideInfo.RomIds, () => overrideInfo.RomIds).HasItems();
                Ensure.That(overrideInfo.Quality, () => overrideInfo.Quality).IsNotNull();
                Ensure.That(overrideInfo.Languages, () => overrideInfo.Languages).IsNotNull();

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

                remoteRom.Game = _gameService.GetGame(overrideInfo.GameId!.Value);
                remoteRom.Roms = _romService.GetRoms(overrideInfo.RomIds);
                remoteRom.ParsedRomInfo.Quality = overrideInfo.Quality;
                remoteRom.Languages = overrideInfo.Languages;
            }

            if (remoteRom.Game == null)
            {
                if (release.SearchInfo?.FileId.HasValue == true)
                {
                    var rom = _romService.GetGameFile(release.SearchInfo.FileId.Value);

                    remoteRom.Game = _gameService.GetGame(rom.GameId);
                    remoteRom.Roms = new List<Rom> { rom };
                }
                else if (release.SearchInfo?.GameId.HasValue == true)
                {
                    var game = _gameService.GetGame(release.SearchInfo.GameId.Value);
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

                if (roms.Empty() && release.SearchInfo?.FileId.HasValue == true)
                {
                    var rom = _romService.GetGameFile(release.SearchInfo.FileId.Value);

                    roms = new List<Rom> { rom };
                }

                remoteRom.Roms = roms;
            }

            if (remoteRom.Roms.Empty())
            {
                throw new RomarrClientException(HttpStatusCode.NotFound, "Unable to parse roms in the release, will need to be manually provided");
            }

            await _downloadService.DownloadReport(remoteRom, release.Override?.DownloadClientId);
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
            var history = _historyService.FindByRomId(romId);

            return MapDecisions(prioritizedDecisions, history);
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
            var history = _historyService.GetByPlatform(gameId, platformNumber, null);

            return MapDecisions(prioritizedDecisions, history);
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

        return MapDecisions(prioritizedDecisions, new List<FileHistory>());
    }

    private string GetCacheKey(ReleaseResource resource)
    {
        return string.Concat(resource.Release!.IndexerId, "_", resource.Release!.Guid);
    }

    private string GetCacheKey(ReleaseGrabResource resource)
    {
        return string.Concat(resource.IndexerId, "_", resource.Guid);
    }

    private List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions, List<FileHistory> history)
    {
        var result = new List<ReleaseResource>();

        foreach (var downloadDecision in decisions)
        {
            var release = downloadDecision.MapDecision(result.Count, _qualityProfile);

            release.History = AddHistory(downloadDecision.RemoteRom.Release, history);
            _remoteRomCache.Set(GetCacheKey(release), downloadDecision.RemoteRom, TimeSpan.FromMinutes(30));

            result.Add(release);
        }

        return result;
    }

    private ReleaseHistoryResource? AddHistory(ReleaseInfo release, List<FileHistory> history)
    {
        var grabbed = history.FirstOrDefault(h => h.EventType == FileHistoryEventType.Grabbed &&
                                                  h.Data.TryGetValue("guid", out var guid) &&
                                                  guid == release.Guid);

        if (grabbed == null && release.DownloadProtocol == DownloadProtocol.Torrent)
        {
            if (release is not TorrentInfo torrentInfo)
            {
                return null;
            }

            if (torrentInfo.InfoHash.IsNotNullOrWhiteSpace())
            {
                grabbed = history.FirstOrDefault(h => h.EventType == FileHistoryEventType.Grabbed &&
                                                      ReleaseComparer.SameTorrent(new ReleaseComparerModel(h),
                                                          torrentInfo));
            }

            if (grabbed == null)
            {
                grabbed = history.FirstOrDefault(h => h.EventType == FileHistoryEventType.Grabbed &&
                                                      h.SourceTitle == release.Title &&
                                                      (DownloadProtocol)Convert.ToInt32(
                                                          h.Data.GetValueOrDefault("protocol")) ==
                                                      DownloadProtocol.Torrent &&
                                                      ReleaseComparer.SameTorrent(new ReleaseComparerModel(h),
                                                          torrentInfo));
            }
        }
        else if (grabbed == null)
        {
            grabbed = history.FirstOrDefault(h => h.EventType == FileHistoryEventType.Grabbed &&
                                                  ReleaseComparer.SameNzb(new ReleaseComparerModel(h),
                                                      release));
        }

        if (grabbed != null)
        {
            var resource = new ReleaseHistoryResource
            {
                Grabbed = grabbed.Date,
            };

            var failedHistory = history.FirstOrDefault(h => h.EventType == FileHistoryEventType.DownloadFailed &&
                                                            h.DownloadId == grabbed.DownloadId);

            if (failedHistory != null)
            {
                resource.Failed = failedHistory.Date;
            }

            return resource;
        }

        return null;
    }
}
