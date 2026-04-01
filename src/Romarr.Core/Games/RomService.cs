using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Cache;
using Romarr.Core.Configuration;
using Romarr.Core.Datastore;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public interface IRomService
    {
        Rom GetGameFile(int id);
        List<Rom> GetRoms(IEnumerable<int> ids);
        Rom FindGameFile(int gameId, int platformNumber, int romNumber);
        Rom FindGameFile(int gameId, int absoluteRomNumber);
        Rom FindGameFileByTitle(int gameId, int platformNumber, string releaseTitle);
        List<Rom> FindGameFilesBySceneNumbering(int gameId, int platformNumber, int romNumber);
        List<Rom> FindGameFilesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber);
        Rom FindGameFile(int gameId, string date, int? part);
        List<Rom> GetGameFileBySeries(int gameId);
        List<Rom> GetRomsByGame(List<int> gameIds);
        List<Rom> GetRomsByPlatform(int gameId, int platformNumber);
        List<Rom> GetRomsByScenePlatform(int gameId, int scenePlatformNumber);
        List<Rom> GameFilesWithFiles(int gameId);
        PagingSpec<Rom> GameFilesWithoutFiles(PagingSpec<Rom> pagingSpec);
        List<Rom> GetRomsByFileId(int romFileId);
        void UpdateGameFile(Rom rom);
        void SetGameFileMonitored(int romId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        void UpdateGameFiles(List<Rom> roms);
        void UpdateLastSearchTime(List<Rom> roms);
        List<Rom> GameFilesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored, bool includeSpecials);
        void InsertMany(List<Rom> roms);
        void UpdateMany(List<Rom> roms);
        void DeleteMany(List<Rom> roms);
        void SetGameFileMonitoredByPlatform(int gameId, int platformNumber, bool monitored);
    }

    public class RomService : IRomService,
                                  IHandle<RomFileDeletedEvent>,
                                  IHandle<RomFileAddedEvent>,
                                  IHandleAsync<GameDeletedEvent>,
                                  IHandleAsync<SeriesScannedEvent>
    {
        private readonly IRomRepository _romRepository;
        private readonly IConfigService _configService;
        private readonly ICached<HashSet<int>> _cache;
        private readonly Logger _logger;

        public RomService(IRomRepository gameFileRepository, IConfigService configService, ICacheManager cacheManager, Logger logger)
        {
            _romRepository = gameFileRepository;
            _configService = configService;
            _cache = cacheManager.GetCache<HashSet<int>>(GetType());
            _logger = logger;
        }

        public Rom GetGameFile(int id)
        {
            return _romRepository.Get(id);
        }

        public List<Rom> GetRoms(IEnumerable<int> ids)
        {
            return _romRepository.Get(ids).ToList();
        }

        public Rom FindGameFile(int gameId, int platformNumber, int romNumber)
        {
            return _romRepository.Find(gameId, platformNumber, romNumber);
        }

        public Rom FindGameFile(int gameId, int absoluteRomNumber)
        {
            return _romRepository.Find(gameId, absoluteRomNumber);
        }

        public List<Rom> FindGameFilesBySceneNumbering(int gameId, int platformNumber, int romNumber)
        {
            return _romRepository.FindGameFilesBySceneNumbering(gameId, platformNumber, romNumber);
        }

        public List<Rom> FindGameFilesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber)
        {
            return _romRepository.FindGameFilesBySceneNumbering(gameId, sceneAbsoluteRomNumber);
        }

        public Rom FindGameFile(int gameId, string date, int? part)
        {
            return FindOneByAirDate(gameId, date, part);
        }

        public List<Rom> GetGameFileBySeries(int gameId)
        {
            return _romRepository.GetRoms(gameId).ToList();
        }

        public List<Rom> GetRomsByGame(List<int> gameIds)
        {
            return _romRepository.GetRomsByGameIds(gameIds).ToList();
        }

        public List<Rom> GetRomsByPlatform(int gameId, int platformNumber)
        {
            return _romRepository.GetRoms(gameId, platformNumber);
        }

        public List<Rom> GetRomsByScenePlatform(int gameId, int scenePlatformNumber)
        {
            return _romRepository.GetRomsByScenePlatform(gameId, scenePlatformNumber);
        }

        public Rom FindGameFileByTitle(int gameId, int platformNumber, string releaseTitle)
        {
            // TODO: can replace this search mechanism with something smarter/faster/better
            var normalizedReleaseTitle = Parser.Parser.NormalizeRomTitle(releaseTitle);
            var cleanNormalizedReleaseTitle = Parser.Parser.CleanGameTitle(normalizedReleaseTitle);
            var roms = _romRepository.GetRoms(gameId, platformNumber);

            var possibleMatches = roms.SelectMany(
                rom => new[]
                {
                    new
                    {
                        Position = normalizedReleaseTitle.IndexOf(Parser.Parser.NormalizeRomTitle(rom.Title), StringComparison.CurrentCultureIgnoreCase),
                        Length = Parser.Parser.NormalizeRomTitle(rom.Title).Length,
                        Rom = rom
                    },
                    new
                    {
                        Position = cleanNormalizedReleaseTitle.IndexOf(Parser.Parser.CleanGameTitle(Parser.Parser.NormalizeRomTitle(rom.Title)), StringComparison.CurrentCultureIgnoreCase),
                        Length = Parser.Parser.NormalizeRomTitle(rom.Title).Length,
                        Rom = rom
                    }
                });

            var matches = possibleMatches
                                .Where(e => e.Rom.Title.Length > 0 && e.Position >= 0)
                                .OrderBy(e => e.Position)
                                .ThenByDescending(e => e.Length)
                                .ToList();

            if (matches.Any())
            {
                return matches.First().Rom;
            }

            return null;
        }

        public List<Rom> GameFilesWithFiles(int gameId)
        {
            return _romRepository.GameFilesWithFiles(gameId);
        }

        public PagingSpec<Rom> GameFilesWithoutFiles(PagingSpec<Rom> pagingSpec)
        {
            var gameFileResult = _romRepository.GameFilesWithoutFiles(pagingSpec, true);

            return gameFileResult;
        }

        public List<Rom> GetRomsByFileId(int romFileId)
        {
            return _romRepository.GetGameFileByFileId(romFileId);
        }

        public void UpdateGameFile(Rom rom)
        {
            _romRepository.Update(rom);
        }

        public void SetGameFileMonitored(int romId, bool monitored)
        {
            var rom = _romRepository.Get(romId);
            _romRepository.SetMonitoredFlat(rom, monitored);

            _logger.Debug("Monitored flag for Rom:{0} was set to {1}", romId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _romRepository.SetMonitored(ids, monitored);
        }

        public void SetGameFileMonitoredByPlatform(int gameId, int platformNumber, bool monitored)
        {
            _romRepository.SetMonitoredByPlatform(gameId, platformNumber, monitored);
        }

        public void UpdateGameFiles(List<Rom> roms)
        {
            _romRepository.UpdateMany(roms);
        }

        public void UpdateLastSearchTime(List<Rom> roms)
        {
            _romRepository.SetFields(roms, e => e.LastSearchTime);
        }

        public List<Rom> GameFilesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored, bool includeSpecials)
        {
            var roms = _romRepository.GameFilesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored, includeSpecials);

            return roms;
        }

        public void InsertMany(List<Rom> roms)
        {
            _romRepository.InsertMany(roms);
        }

        public void UpdateMany(List<Rom> roms)
        {
            _romRepository.UpdateMany(roms);
        }

        public void DeleteMany(List<Rom> roms)
        {
            _romRepository.DeleteMany(roms);
        }

        private Rom FindOneByAirDate(int gameId, string date, int? part)
        {
            var roms = _romRepository.Find(gameId, date);

            if (!roms.Any())
            {
                return null;
            }

            if (roms.Count == 1)
            {
                return roms.First();
            }

            _logger.Debug("Multiple roms with the same air date were found, will exclude specials");

            var regularGameFiles = roms.Where(e => e.PlatformNumber > 0).ToList();

            if (regularGameFiles.Count == 1 && !part.HasValue)
            {
                _logger.Debug("Left with one rom after excluding specials");
                return regularGameFiles.First();
            }
            else if (part.HasValue && part.Value <= regularGameFiles.Count)
            {
                var sortedGameFiles = regularGameFiles.OrderBy(e => e.PlatformNumber)
                                                               .ThenBy(e => e.FileNumber)
                                                                .ToList();

                return sortedGameFiles[part.Value - 1];
            }

            throw new InvalidOperationException($"Multiple roms with the same air date found. Date: {date}");
        }

        public void Handle(RomFileDeletedEvent message)
        {
            foreach (var rom in GetRomsByFileId(message.RomFile.Id))
            {
                _logger.Debug("Detaching rom {0} from file.", rom.Id);

                var unmonitorGameFiles = _configService.AutoUnmonitorPreviouslyDownloadedGameFiles;

                var unmonitorForReason = message.Reason != DeleteMediaFileReason.Upgrade &&
                                         message.Reason != DeleteMediaFileReason.ManualOverride &&
                                         message.Reason != DeleteMediaFileReason.MissingFromDisk;

                // If rom is being unlinked because it's missing from disk store it for
                if (message.Reason == DeleteMediaFileReason.MissingFromDisk && unmonitorGameFiles)
                {
                    lock (_cache)
                    {
                        var ids = _cache.Get(rom.GameId.ToString(), () => new HashSet<int>());

                        ids.Add(rom.Id);
                    }
                }

                _romRepository.ClearFileId(rom, unmonitorForReason && unmonitorGameFiles);
            }
        }

        public void Handle(RomFileAddedEvent message)
        {
            foreach (var rom in message.RomFile.Roms.Value)
            {
                _romRepository.SetFileId(rom, message.RomFile.Id);

                lock (_cache)
                {
                    var ids = _cache.Find(rom.GameId.ToString());

                    if (ids?.Contains(rom.Id) == true)
                    {
                        ids.Remove(rom.Id);
                    }
                }

                _logger.Debug("Linking [{0}] > [{1}]", message.RomFile.RelativePath, rom);
            }
        }

        public void HandleAsync(GameDeletedEvent message)
        {
            var roms = _romRepository.GetRomsByGameIds(message.Game.Select(s => s.Id).ToList());
            _romRepository.DeleteMany(roms);
        }

        public void HandleAsync(SeriesScannedEvent message)
        {
            lock (_cache)
            {
                var ids = _cache.Find(message.Game.Id.ToString());

                if (ids?.Any() == true)
                {
                    _romRepository.SetMonitored(ids, false);
                }

                _cache.Remove(message.Game.Id.ToString());
            }
        }
    }
}
