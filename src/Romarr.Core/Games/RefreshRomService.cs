using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public interface IRefreshRomService
    {
        void RefreshRomInfo(Game game, IEnumerable<Rom> remoteRoms);
    }

    public class RefreshRomService : IRefreshRomService
    {
        private readonly IRomService _romService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshRomService(IRomService gameFileService, IEventAggregator eventAggregator, Logger logger)
        {
            _romService = gameFileService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshRomInfo(Game game, IEnumerable<Rom> remoteRoms)
        {
            _logger.Info("Starting rom info refresh for: {0}", game);
            var successCount = 0;
            var failCount = 0;

            var existingRoms = _romService.GetGameFileBySeries(game.Id);
            var platforms = game.Platforms;
            var hasExisting = existingRoms.Any();

            var updateList = new List<Rom>();
            var newList = new List<Rom>();
            var dupeFreeRemoteGameFiles = remoteRoms.DistinctBy(m => new { m.PlatformNumber, m.FileNumber }).ToList();

            var orderedGameFiles = OrderGameFiles(game, dupeFreeRemoteGameFiles).ToList();
            var gameFilesPerPlatform = orderedGameFiles.GroupBy(s => s.PlatformNumber).ToDictionary(g => g.Key, g => g.Count());
            var latestPlatform = platforms.MaxBy(s => s.PlatformNumber);

            foreach (var rom in orderedGameFiles)
            {
                try
                {
                    var gameFileToUpdate = existingRoms.FirstOrDefault(e => e.PlatformNumber == rom.PlatformNumber && e.FileNumber == rom.FileNumber);

                    if (gameFileToUpdate != null)
                    {
                        existingRoms.Remove(gameFileToUpdate);
                        updateList.Add(gameFileToUpdate);
                    }
                    else
                    {
                        gameFileToUpdate = new Rom();
                        gameFileToUpdate.Monitored = GetMonitoredStatus(rom, platforms, game);
                        newList.Add(gameFileToUpdate);
                    }

                    gameFileToUpdate.GameId = game.Id;
                    gameFileToUpdate.IgdbId = rom.IgdbId;
                    gameFileToUpdate.FileNumber = rom.FileNumber;
                    gameFileToUpdate.PlatformNumber = rom.PlatformNumber;
                    gameFileToUpdate.AbsoluteFileNumber = rom.AbsoluteFileNumber;
                    gameFileToUpdate.AiredAfterPlatformNumber = rom.AiredAfterPlatformNumber;
                    gameFileToUpdate.AiredBeforePlatformNumber = rom.AiredBeforePlatformNumber;
                    gameFileToUpdate.AiredBeforeRomNumber = rom.AiredBeforeRomNumber;
                    gameFileToUpdate.Title = rom.Title ?? "TBA";
                    gameFileToUpdate.Overview = rom.Overview;
                    gameFileToUpdate.AirDate = rom.AirDate;
                    gameFileToUpdate.AirDateUtc = rom.AirDateUtc;
                    gameFileToUpdate.Runtime = rom.Runtime;
                    gameFileToUpdate.FinaleType = rom.FinaleType;
                    gameFileToUpdate.RomType = rom.RomType;
                    gameFileToUpdate.Ratings = rom.Ratings;
                    gameFileToUpdate.Images = rom.Images;

                    // TheIGDB has a severe lack of platform/game finales, this helps smooth out that limitation so they can be displayed in the UI
                    if (game.Status == GameStatusType.Ended &&
                        gameFileToUpdate.FinaleType == null &&
                        gameFileToUpdate.PlatformNumber > 0 &&
                        gameFileToUpdate.PlatformNumber == latestPlatform.PlatformNumber &&
                        gameFileToUpdate.FileNumber > 1 &&
                        gameFileToUpdate.FileNumber == gameFilesPerPlatform[gameFileToUpdate.PlatformNumber] &&
                        gameFileToUpdate.AirDateUtc.HasValue &&
                        gameFileToUpdate.AirDateUtc.Value.After(DateTime.UtcNow.AddDays(-14)) &&
                        orderedGameFiles.None(e => e.PlatformNumber == latestPlatform.PlatformNumber && e.FinaleType != null))
                    {
                        gameFileToUpdate.FinaleType = "game";
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating rom info for game {0}. {1}", game, rom);
                    failCount++;
                }
            }

            UnmonitorReaddedGameFiles(game, newList, hasExisting);

            var allGameFiles = new List<Rom>();
            allGameFiles.AddRange(newList);
            allGameFiles.AddRange(updateList);

            AdjustMultiGameFileAirTime(game, allGameFiles);
            AdjustDirectToDvdAirDate(game, allGameFiles);

            _romService.DeleteMany(existingRoms);
            _romService.UpdateMany(updateList);
            _romService.InsertMany(newList);

            _eventAggregator.PublishEvent(new RomInfoRefreshedEvent(game, newList, updateList, existingRoms));

            if (failCount != 0)
            {
                _logger.Info("Finished rom refresh for game: {0}. Successful: {1} - Failed: {2} ",
                    game.Title,
                    successCount,
                    failCount);
            }
            else
            {
                _logger.Info("Finished rom refresh for game: {0}.", game);
            }
        }

        private bool GetMonitoredStatus(Rom rom, IEnumerable<Platform> platforms, Game game)
        {
            if (rom.FileNumber == 0 && rom.PlatformNumber != 1)
            {
                return false;
            }

            var platform = platforms.SingleOrDefault(c => c.PlatformNumber == rom.PlatformNumber);
            return platform == null || platform.Monitored;
        }

        private void UnmonitorReaddedGameFiles(Game game, List<Rom> roms, bool hasExisting)
        {
            if (game.AddOptions != null)
            {
                return;
            }

            var threshold = DateTime.UtcNow.AddDays(-14);

            var oldGameFiles = roms.Where(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(threshold)).ToList();

            if (oldGameFiles.Any())
            {
                if (hasExisting)
                {
                    _logger.Warn("Show {0} ({1}) had {2} old roms appear, please check monitored status.", game.IgdbId, game.Title, oldGameFiles.Count);
                }
                else
                {
                    threshold = DateTime.UtcNow.AddDays(-1);

                    foreach (var rom in roms)
                    {
                        if (rom.AirDateUtc.HasValue && rom.AirDateUtc.Value.Before(threshold))
                        {
                            rom.Monitored = false;
                        }
                    }

                    _logger.Warn("Show {0} ({1}) had {2} old roms appear, unmonitored aired roms to prevent unexpected downloads.", game.IgdbId, game.Title, oldGameFiles.Count);
                }
            }
        }

        private void AdjustMultiGameFileAirTime(Game game, IEnumerable<Rom> allGameFiles)
        {
            var groups = allGameFiles.Where(c => c.AirDateUtc.HasValue)
                                    .GroupBy(e => new { e.PlatformNumber, e.AirDate })
                                    .Where(g => g.Count() > 1)
                                    .ToList();

            foreach (var group in groups)
            {
                if (group.Key.PlatformNumber != 0 && group.Count() > 3)
                {
                    _logger.Debug("Not adjusting multi-rom air times for game {0} platform {1} since more than 3 roms 'aired' on the same day", game.Title, group.Key.PlatformNumber);
                    continue;
                }

                var gameFileCount = 0;

                foreach (var rom in group.OrderBy(e => e.PlatformNumber).ThenBy(e => e.FileNumber))
                {
                    rom.AirDateUtc = rom.AirDateUtc.Value.AddMinutes(game.Runtime * gameFileCount);
                    gameFileCount++;
                }
            }
        }

        private void AdjustDirectToDvdAirDate(Game game, IList<Rom> allGameFiles)
        {
            if (game.Status == GameStatusType.Ended && allGameFiles.All(v => !v.AirDateUtc.HasValue) && game.FirstAired.HasValue)
            {
                foreach (var rom in allGameFiles)
                {
                    rom.AirDateUtc = game.FirstAired;
                    rom.AirDate = game.FirstAired.Value.ToString("yyyy-MM-dd");
                }
            }
        }

        private List<Rom> MapAbsoluteRomNumbers(List<Rom> remoteRoms)
        {
            // Return all roms with no abs number, but distinct for those with abs number
            return remoteRoms.Where(e => e.AbsoluteFileNumber.HasValue)
                                 .OrderByDescending(e => e.PlatformNumber)
                                 .DistinctBy(e => e.AbsoluteFileNumber.Value)
                                 .Concat(remoteRoms.Where(e => !e.AbsoluteFileNumber.HasValue))
                                 .ToList();
        }

        private IEnumerable<Rom> OrderGameFiles(Game game, List<Rom> roms)
        {
            return roms.OrderBy(e => e.PlatformNumber).ThenBy(e => e.FileNumber);
        }
    }
}
