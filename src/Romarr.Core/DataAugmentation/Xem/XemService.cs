using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Cache;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games;
using Romarr.Core.Games.Events;

namespace Romarr.Core.DataAugmentation.Xem
{
    public class XemService : ISceneMappingProvider, IHandle<GameUpdatedEvent>, IHandle<GameRefreshStartingEvent>
    {
        private readonly IRomService _romService;
        private readonly IXemProxy _xemProxy;
        private readonly IGameService _gameService;
        private readonly Logger _logger;
        private readonly ICachedDictionary<bool> _cache;

        public XemService(IRomService gameFileService,
                           IXemProxy xemProxy,
                           IGameService seriesService,
                           ICacheManager cacheManager,
                           Logger logger)
        {
            _romService = gameFileService;
            _xemProxy = xemProxy;
            _gameService = seriesService;
            _logger = logger;
            _cache = cacheManager.GetCacheDictionary<bool>(GetType(), "mappedIgdbid");
        }

        private void PerformUpdate(Game game)
        {
            _logger.Debug("Updating scene numbering mapping for: {0}", game);

            try
            {
                var mappings = _xemProxy.GetSceneIgdbMappings(game.IgdbId);

                if (!mappings.Any() && !game.UseSceneNumbering)
                {
                    _logger.Debug("Mappings for: {0} are empty, skipping", game);
                    return;
                }

                var roms = _romService.GetGameFileBySeries(game.Id);

                foreach (var rom in roms)
                {
                    rom.SceneAbsoluteFileNumber = null;
                    rom.ScenePlatformNumber = null;
                    rom.SceneFileNumber = null;
                    rom.UnverifiedSceneNumbering = false;
                }

                foreach (var mapping in mappings)
                {
                    _logger.Debug("Setting scene numbering mappings for {0} S{1:00}E{2:00}", game, mapping.Igdb.Platform, mapping.Igdb.Rom);

                    var rom = roms.SingleOrDefault(e => e.PlatformNumber == mapping.Igdb.Platform && e.FileNumber == mapping.Igdb.Rom);

                    if (rom == null)
                    {
                        _logger.Debug("Information hasn't been added to TheIGDB yet, skipping");
                        continue;
                    }

                    if (mapping.Scene.Absolute == 0 &&
                        mapping.Scene.Platform == 0 &&
                        mapping.Scene.Rom == 0)
                    {
                        _logger.Debug("Mapping for {0} S{1:00}E{2:00} is invalid, skipping", game, mapping.Igdb.Platform, mapping.Igdb.Rom);
                        continue;
                    }

                    rom.SceneAbsoluteFileNumber = mapping.Scene.Absolute;
                    rom.ScenePlatformNumber = mapping.Scene.Platform;
                    rom.SceneFileNumber = mapping.Scene.Rom;
                }

                if (roms.Any(v => v.SceneFileNumber.HasValue && v.ScenePlatformNumber != 0))
                {
                    ExtrapolateMappings(game, roms, mappings);
                }

                _romService.UpdateGameFiles(roms);
                game.UseSceneNumbering = mappings.Any();
                _gameService.UpdateSeries(game);

                _logger.Debug("XEM mapping updated for {0}", game);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating scene numbering mappings for {0}", game);
            }
        }

        private void ExtrapolateMappings(Game game, List<Rom> roms, List<Model.XemSceneIgdbMapping> mappings)
        {
            var mappedGameFiles = roms.Where(v => v.PlatformNumber != 0 && v.SceneFileNumber.HasValue).ToList();
            var mappedPlatforms = new HashSet<int>(mappedGameFiles.Select(v => v.PlatformNumber).Distinct());

            var sceneGameFileMappings = mappings.ToLookup(v => v.Scene.Platform)
                                               .ToDictionary(v => v.Key, e => new HashSet<int>(e.Select(v => v.Scene.Rom)));

            var firstIgdbGameFileByPlatform = mappings.ToLookup(v => v.Igdb.Platform)
                                                   .ToDictionary(v => v.Key, e => e.Min(v => v.Igdb.Rom));

            var lastScenePlatform = mappings.Select(v => v.Scene.Platform).Max();
            var lastIgdbPlatform = mappings.Select(v => v.Igdb.Platform).Max();

            // Mark all roms not on the xem as unverified.
            foreach (var rom in roms)
            {
                if (rom.PlatformNumber == 0)
                {
                    continue;
                }

                if (rom.SceneFileNumber.HasValue)
                {
                    continue;
                }

                if (mappedPlatforms.Contains(rom.PlatformNumber))
                {
                    // Mark if a mapping exists for an earlier rom in this platform.
                    if (firstIgdbGameFileByPlatform[rom.PlatformNumber] <= rom.FileNumber)
                    {
                        rom.UnverifiedSceneNumbering = true;
                        continue;
                    }

                    // Mark if a mapping exists with a scene number to this rom.
                    if (sceneGameFileMappings.ContainsKey(rom.PlatformNumber) &&
                        sceneGameFileMappings[rom.PlatformNumber].Contains(rom.FileNumber))
                    {
                        rom.UnverifiedSceneNumbering = true;
                        continue;
                    }
                }
                else if (lastScenePlatform != lastIgdbPlatform && rom.PlatformNumber > lastIgdbPlatform)
                {
                    rom.UnverifiedSceneNumbering = true;
                }
            }

            foreach (var rom in roms)
            {
                if (rom.PlatformNumber == 0)
                {
                    continue;
                }

                if (rom.SceneFileNumber.HasValue)
                {
                    continue;
                }

                if (rom.PlatformNumber < lastIgdbPlatform)
                {
                    continue;
                }

                if (!rom.UnverifiedSceneNumbering)
                {
                    continue;
                }

                var platformMappings = mappings.Where(v => v.Igdb.Platform == rom.PlatformNumber).ToList();
                if (platformMappings.Any(v => v.Igdb.Rom >= rom.FileNumber))
                {
                    continue;
                }

                if (platformMappings.Any())
                {
                    var lastGameFileMapping = platformMappings.OrderBy(v => v.Igdb.Rom).Last();
                    var lastScenePlatformMapping = mappings.Where(v => v.Scene.Platform == lastGameFileMapping.Scene.Platform).OrderBy(v => v.Scene.Rom).Last();

                    if (lastScenePlatformMapping.Igdb.Platform == 0)
                    {
                        continue;
                    }

                    var offset = rom.FileNumber - lastGameFileMapping.Igdb.Rom;

                    rom.ScenePlatformNumber = lastGameFileMapping.Scene.Platform;
                    rom.SceneFileNumber = lastGameFileMapping.Scene.Rom + offset;
                    rom.SceneAbsoluteFileNumber = lastGameFileMapping.Scene.Absolute + offset;
                }
                else if (lastIgdbPlatform != lastScenePlatform)
                {
                    var offset = rom.PlatformNumber - lastIgdbPlatform;

                    rom.ScenePlatformNumber = lastScenePlatform + offset;
                    rom.SceneFileNumber = rom.FileNumber;

                    // TODO: SceneAbsoluteFileNumber.
                }
            }
        }

        private void UpdateXemGameIds()
        {
            try
            {
                var ids = _xemProxy.GetXemGameIds();

                if (ids.Any())
                {
                    _cache.Update(ids.ToDictionary(v => v.ToString(), v => true));
                    return;
                }

                _cache.ExtendTTL();
                _logger.Warn("Failed to update Xem game list.");
            }
            catch (Exception ex)
            {
                _cache.ExtendTTL();
                _logger.Warn(ex, "Failed to update Xem game list.");
            }
        }

        public List<SceneMapping> GetSceneMappings()
        {
            var mappings = _xemProxy.GetSceneIgdbNames();

            return mappings;
        }

        public void Handle(GameUpdatedEvent message)
        {
            if (_cache.IsExpired(TimeSpan.FromHours(3)))
            {
                UpdateXemGameIds();
            }

            if (_cache.Count == 0)
            {
                _logger.Debug("Scene numbering is not available");
                return;
            }

            if (!_cache.Find(message.Game.IgdbId.ToString()) && !message.Game.UseSceneNumbering)
            {
                _logger.Debug("Scene numbering is not available for {0} [{1}]", message.Game.Title, message.Game.IgdbId);
                return;
            }

            PerformUpdate(message.Game);
        }

        public void Handle(GameRefreshStartingEvent message)
        {
            if (message.ManualTrigger && _cache.IsExpired(TimeSpan.FromMinutes(1)))
            {
                UpdateXemGameIds();
            }
        }
    }
}
