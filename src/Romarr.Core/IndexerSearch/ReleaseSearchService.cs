using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Common.Instrumentation.Extensions;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.DecisionEngine;
using Romarr.Core.Indexers;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        Task<List<DownloadDecision>> RomSearch(int romId, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> RomSearch(Rom rom, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> PlatformSearch(int gameId, int platformNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> PlatformSearch(int gameId, int platformNumber, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
    }

    public class ReleaseSearchService : ISearchForReleases
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly ISceneMappingService _sceneMapping;
        private readonly IGameService _gameService;
        private readonly IRomService _romService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public ReleaseSearchService(IIndexerFactory indexerFactory,
                                ISceneMappingService sceneMapping,
                                IGameService seriesService,
                                IRomService gameFileService,
                                IMakeDownloadDecision makeDownloadDecision,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _sceneMapping = sceneMapping;
            _gameService = seriesService;
            _romService = gameFileService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public async Task<List<DownloadDecision>> RomSearch(int romId, bool userInvokedSearch, bool interactiveSearch)
        {
            var rom = _romService.GetGameFile(romId);

            return await RomSearch(rom, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> RomSearch(Rom rom, bool userInvokedSearch, bool interactiveSearch)
        {
            var game = _gameService.GetGame(rom.GameId);

            if (rom.PlatformNumber == 0)
            {
                // Search for special roms in platform 0
                return await SearchSpecial(game, new List<Rom> { rom }, false, userInvokedSearch, interactiveSearch);
            }

            return await SearchSingle(game, rom, false, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> PlatformSearch(int gameId, int platformNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var roms = _romService.GetRomsByPlatform(gameId, platformNumber);

            if (missingOnly)
            {
                roms = roms.Where(e => !e.HasFile).ToList();
            }

            if (roms.Count == 0)
            {
                _logger.Debug("No wanted roms found for platform {0}", platformNumber);
                return new List<DownloadDecision>();
            }

            return await PlatformSearch(gameId, platformNumber, roms, monitoredOnly, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> PlatformSearch(int gameId, int platformNumber, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var game = _gameService.GetGame(gameId);

            var mappings = GetScenePlatformMappings(game, roms);

            var downloadDecisions = new List<DownloadDecision>();

            foreach (var mapping in mappings)
            {
                if (mapping.PlatformNumber == 0)
                {
                    // search for special roms in platform 0
                    downloadDecisions.AddRange(await SearchSpecial(game, mapping.Roms, monitoredOnly, userInvokedSearch, interactiveSearch));
                    continue;
                }

                if (mapping.Roms.Count == 1)
                {
                    var searchSpec = Get<SingleGameFileSearchCriteria>(game, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.PlatformNumber = mapping.PlatformNumber;
                    searchSpec.FileNumber = mapping.GameFileMapping.FileNumber;

                    var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                    downloadDecisions.AddRange(decisions);
                }
                else
                {
                    var searchSpec = Get<PlatformSearchCriteria>(game, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.PlatformNumber = mapping.PlatformNumber;

                    var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                    downloadDecisions.AddRange(decisions);
                }
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private List<ScenePlatformMapping> GetScenePlatformMappings(Game game, List<Rom> roms)
        {
            var dict = new Dictionary<ScenePlatformMapping, ScenePlatformMapping>();

            var sceneMappings = _sceneMapping.FindByIgdbId(game.IgdbId);

            // Group the rom by ScenePlatformNumber/PlatformNumber, in 99% of cases this will result in 1 groupedGameFile
            var groupedGameFiles = roms.ToLookup(v => ((v.ScenePlatformNumber ?? v.PlatformNumber) * 100000) + v.PlatformNumber);

            foreach (var groupedGameFile in groupedGameFiles)
            {
                var gameFileMappings = GetSceneGameFileMappings(game, groupedGameFile.First(), sceneMappings);

                foreach (var gameFileMapping in gameFileMappings)
                {
                    var platformMapping = new ScenePlatformMapping
                    {
                        Roms = groupedGameFile.ToList(),
                        GameFileMapping = gameFileMapping,
                        SceneTitles = gameFileMapping.SceneTitles,
                        SearchMode = gameFileMapping.SearchMode,
                        PlatformNumber = gameFileMapping.PlatformNumber
                    };

                    if (dict.TryGetValue(platformMapping, out var existing))
                    {
                        existing.Roms.AddRange(platformMapping.Roms);
                        existing.SceneTitles.AddRange(platformMapping.SceneTitles);
                    }
                    else
                    {
                        dict[platformMapping] = platformMapping;
                    }
                }
            }

            foreach (var item in dict)
            {
                item.Value.Roms = item.Value.Roms.Distinct().ToList();
                item.Value.SceneTitles = item.Value.SceneTitles.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            }

            return dict.Values.ToList();
        }

        private List<SceneGameFileMapping> GetSceneGameFileMappings(Game game, Rom rom)
        {
            var dict = new Dictionary<SceneGameFileMapping, SceneGameFileMapping>();

            var sceneMappings = _sceneMapping.FindByIgdbId(game.IgdbId);

            var gameFileMappings = GetSceneGameFileMappings(game, rom, sceneMappings);

            foreach (var gameFileMapping in gameFileMappings)
            {
                if (dict.TryGetValue(gameFileMapping, out var existing))
                {
                    existing.SceneTitles.AddRange(gameFileMapping.SceneTitles);
                }
                else
                {
                    dict[gameFileMapping] = gameFileMapping;
                }
            }

            foreach (var item in dict)
            {
                item.Value.SceneTitles = item.Value.SceneTitles.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            }

            return dict.Values.ToList();
        }

        private IEnumerable<SceneGameFileMapping> GetSceneGameFileMappings(Game game, Rom rom, List<SceneMapping> sceneMappings)
        {
            var includeGlobal = true;

            foreach (var sceneMapping in sceneMappings)
            {
                // There are two kinds of mappings:
                // - Mapped on Release Platform Number with sceneMapping.ScenePlatformNumber specified and optionally sceneMapping.PlatformNumber. This translates via rom.ScenePlatformNumber/PlatformNumber to specific roms.
                // - Mapped on Rom Platform Number with optionally sceneMapping.PlatformNumber. This translates from rom.ScenePlatformNumber/PlatformNumber to specific releases. (Filter by rom.PlatformNumber or globally)

                var ignoreSceneNumbering = sceneMapping.SceneOrigin == "igdb" || sceneMapping.SceneOrigin == "unknown:igdb";
                var mappingScenePlatformNumber = sceneMapping.ScenePlatformNumber.NonNegative();
                var mappingPlatformNumber = sceneMapping.PlatformNumber.NonNegative();

                // Select scene or igdb on the rom
                var mappedPlatformNumber = ignoreSceneNumbering ? rom.PlatformNumber : (rom.ScenePlatformNumber ?? rom.PlatformNumber);
                var releasePlatformNumber = sceneMapping.ScenePlatformNumber.NonNegative() ?? mappedPlatformNumber;

                if (mappingScenePlatformNumber.HasValue)
                {
                    // Apply the alternative mapping (release to scene/igdb)
                    var mappedAltPlatformNumber = sceneMapping.PlatformNumber.NonNegative() ?? sceneMapping.ScenePlatformNumber.NonNegative() ?? mappedPlatformNumber;

                    // Check if the mapping applies to the current platform
                    if (mappedAltPlatformNumber != mappedPlatformNumber)
                    {
                        continue;
                    }
                }
                else
                {
                    // Check if the mapping applies to the current platform
                    if (mappingPlatformNumber.HasValue && mappingPlatformNumber.Value != rom.PlatformNumber)
                    {
                        continue;
                    }
                }

                if (sceneMapping.SearchTerm == game.Title && sceneMapping.FilterRegex.IsNullOrWhiteSpace())
                {
                    // Disable the implied mapping if we have an explicit mapping by the same name
                    includeGlobal = false;
                }

                // By default we do a alt title search in case indexers don't have the release properly indexed.  Services can override this behavior.
                var searchMode = sceneMapping.SearchMode ?? ((mappingScenePlatformNumber.HasValue && game.CleanTitle != sceneMapping.SearchTerm.CleanGameTitle()) ? SearchMode.SearchTitle : SearchMode.Default);

                if (ignoreSceneNumbering)
                {
                    yield return new SceneGameFileMapping
                    {
                        Rom = rom,
                        SearchMode = searchMode,
                        SceneTitles = new List<string> { sceneMapping.SearchTerm },
                        PlatformNumber = releasePlatformNumber,
                        FileNumber = rom.FileNumber,
                        AbsoluteFileNumber = rom.AbsoluteFileNumber
                    };
                }
                else
                {
                    yield return new SceneGameFileMapping
                    {
                        Rom = rom,
                        SearchMode = searchMode,
                        SceneTitles = new List<string> { sceneMapping.SearchTerm },
                        PlatformNumber = releasePlatformNumber,
                        FileNumber = rom.SceneFileNumber ?? rom.FileNumber,
                        AbsoluteFileNumber = rom.SceneAbsoluteFileNumber ?? rom.AbsoluteFileNumber
                    };
                }
            }

            if (includeGlobal)
            {
                yield return new SceneGameFileMapping
                {
                    Rom = rom,
                    SearchMode = SearchMode.Default,
                    SceneTitles = new List<string> { game.Title },
                    PlatformNumber = rom.ScenePlatformNumber ?? rom.PlatformNumber,
                    FileNumber = rom.SceneFileNumber ?? rom.FileNumber,
                    AbsoluteFileNumber = rom.ScenePlatformNumber ?? rom.AbsoluteFileNumber
                };
            }
        }

        private async Task<List<DownloadDecision>> SearchSingle(Game game, Rom rom, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var mappings = GetSceneGameFileMappings(game, rom);

            var downloadDecisions = new List<DownloadDecision>();

            foreach (var mapping in mappings)
            {
                var searchSpec = Get<SingleGameFileSearchCriteria>(game, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                searchSpec.PlatformNumber = mapping.PlatformNumber;
                searchSpec.FileNumber = mapping.FileNumber;

                var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchSpecial(Game game, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<SpecialGameFileSearchCriteria>(game, roms, monitoredOnly, userInvokedSearch, interactiveSearch);

            // build list of queries for each rom in the form: "<game> <rom-title>"
            searchSpec.GameFileQueryTitles = roms.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                                                    .Where(e => interactiveSearch || !monitoredOnly || e.Monitored)
                                                    .SelectMany(e => searchSpec.CleanSceneTitles.Select(title => title + " " + SearchCriteriaBase.GetCleanSceneTitle(e.Title)))
                                                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                                    .ToArray();

            downloadDecisions.AddRange(await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));

            // Search for each rom by platform/rom number as well
            foreach (var rom in roms)
            {
                // Rom needs to be monitored if it's not an interactive search
                if (!interactiveSearch && monitoredOnly && !rom.Monitored)
                {
                    continue;
                }

                downloadDecisions.AddRange(await SearchSingle(game, rom, monitoredOnly, userInvokedSearch, interactiveSearch));
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private TSpec Get<TSpec>(Game game, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Game = game;
            spec.SceneTitles = _sceneMapping.GetSceneNames(game.IgdbId,
                                                           roms.Select(e => e.PlatformNumber).Distinct().ToList(),
                                                           roms.Select(e => e.ScenePlatformNumber ?? e.PlatformNumber).Distinct().ToList());

            spec.Roms = roms;
            spec.MonitoredGameFilesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            if (!spec.SceneTitles.Contains(game.Title, StringComparer.InvariantCultureIgnoreCase))
            {
                spec.SceneTitles.Add(game.Title);
            }

            return spec;
        }

        private TSpec Get<TSpec>(Game game, SceneGameFileMapping mapping, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Game = game;
            spec.SceneTitles = mapping.SceneTitles;
            spec.SearchMode = mapping.SearchMode;

            spec.Roms = new List<Rom> { mapping.Rom };
            spec.MonitoredGameFilesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private TSpec Get<TSpec>(Game game, ScenePlatformMapping mapping, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Game = game;
            spec.SceneTitles = mapping.SceneTitles;
            spec.SearchMode = mapping.SearchMode;

            spec.Roms = mapping.Roms;
            spec.MonitoredGameFilesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private async Task<List<DownloadDecision>> Dispatch(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Game.Tags).Any()).ToList();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

            var tasks = indexers.Select(indexer => DispatchIndexer(searchAction, indexer, criteriaBase));

            var batch = await Task.WhenAll(tasks);

            var reports = batch.SelectMany(x => x).ToList();

            _logger.ProgressDebug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            // Update the last search time for all roms if at least 1 indexer was searched.
            if (indexers.Any())
            {
                var lastSearchTime = DateTime.UtcNow;
                _logger.Debug("Setting last search time to: {0}", lastSearchTime);

                criteriaBase.Roms.ForEach(e => e.LastSearchTime = lastSearchTime);
                _romService.UpdateLastSearchTime(criteriaBase.Roms);
            }

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }

        private async Task<IList<ReleaseInfo>> DispatchIndexer(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, IIndexer indexer, SearchCriteriaBase criteriaBase)
        {
            try
            {
                return await searchAction(indexer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while searching for {0}", criteriaBase);
            }

            return Array.Empty<ReleaseInfo>();
        }

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections and higher indexer priority.
            return decisions.GroupBy(d => d.RemoteRom.Release.Guid)
                .Select(d => d.OrderBy(v => v.Rejections.Count()).ThenBy(v => v.RemoteRom?.Release?.IndexerPriority ?? IndexerDefinition.DefaultPriority).First())
                .ToList();
        }
    }
}
