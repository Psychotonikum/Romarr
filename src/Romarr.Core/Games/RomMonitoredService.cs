using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.MediaFiles;

namespace Romarr.Core.Games
{
    public interface IFileMonitoredService
    {
        void SetGameFileMonitoredStatus(Game game, MonitoringOptions monitoringOptions);
    }

    public class FileMonitoredService : IFileMonitoredService
    {
        private readonly IGameService _gameService;
        private readonly IRomService _romService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public FileMonitoredService(IGameService seriesService, IRomService gameFileService, IMediaFileService mediaFileService, Logger logger)
        {
            _gameService = seriesService;
            _romService = gameFileService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public void SetGameFileMonitoredStatus(Game game, MonitoringOptions monitoringOptions)
        {
            // Update the game without changing the roms
            if (monitoringOptions == null)
            {
                _gameService.UpdateSeries(game, false);
                return;
            }

            // Fallback for v2 endpoints
            if (monitoringOptions.Monitor == MonitorTypes.Unknown)
            {
                LegacySetGameFileMonitoredStatus(game, monitoringOptions);
                return;
            }

            // Skip rom level monitoring and use platform information when game was added
            if (monitoringOptions.Monitor == MonitorTypes.Skip)
            {
                return;
            }

            var firstPlatform = game.Platforms.Select(s => s.PlatformNumber).Where(s => s > 0).MinOrDefault();
            var lastPlatform = game.Platforms.Select(s => s.PlatformNumber).MaxOrDefault();
            var roms = _romService.GetGameFileBySeries(game.Id);

            switch (monitoringOptions.Monitor)
            {
                case MonitorTypes.All:
                    _logger.Debug("[{0}] Monitoring all roms", game.Title);
                    ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0);

                    break;

                case MonitorTypes.Future:
                    _logger.Debug("[{0}] Monitoring future roms", game.Title);
                    ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && (!e.AirDateUtc.HasValue || e.AirDateUtc >= DateTime.UtcNow));

                    break;

                case MonitorTypes.Missing:
                    _logger.Debug("[{0}] Monitoring missing roms", game.Title);
                    ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && !e.HasFile);

                    break;

                case MonitorTypes.Existing:
                    _logger.Debug("[{0}] Monitoring existing roms", game.Title);
                    ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && e.HasFile);

                    break;

                case MonitorTypes.FirstPlatform:
                    _logger.Debug("[{0}] Monitoring first platform roms", game.Title);
                    ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && e.PlatformNumber == firstPlatform);

                    break;

                case MonitorTypes.LastPlatform:
                    _logger.Debug("[{0}] Monitoring latest platform roms", game.Title);

                    ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && e.PlatformNumber == lastPlatform);

                    break;

                case MonitorTypes.None:
                    _logger.Debug("[{0}] Unmonitoring all roms", game.Title);
                    ToggleGameFilesMonitoredState(roms, e => false);

                    break;

                case MonitorTypes.BaseGame:
                    _logger.Debug("[{0}] Monitoring base game roms only", game.Title);
                    {
                        var files = _mediaFileService.GetFilesBySeries(game.Id);
                        var baseFileIds = files.Where(f => f.RomFileType == RomFileType.Base && (RomReleaseType)f.RomReleaseType == RomReleaseType.Retail)
                                               .Select(f => f.Id)
                                               .ToHashSet();
                        ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && (!e.HasFile || baseFileIds.Contains(e.RomFileId)));
                    }

                    break;

                case MonitorTypes.AllDlcs:
                    _logger.Debug("[{0}] Monitoring all DLC roms", game.Title);
                    {
                        var files = _mediaFileService.GetFilesBySeries(game.Id);
                        var dlcFileIds = files.Where(f => f.RomFileType == RomFileType.Dlc || (RomReleaseType)f.RomReleaseType == RomReleaseType.Dlc)
                                              .Select(f => f.Id)
                                              .ToHashSet();
                        ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && (!e.HasFile || dlcFileIds.Contains(e.RomFileId)));
                    }

                    break;

                case MonitorTypes.LatestUpdate:
                    _logger.Debug("[{0}] Monitoring latest update rom", game.Title);
                    {
                        var files = _mediaFileService.GetFilesBySeries(game.Id);
                        var updateFiles = files.Where(f => f.RomFileType == RomFileType.Update || (RomReleaseType)f.RomReleaseType == RomReleaseType.Update)
                                               .OrderByDescending(f => f.PatchVersion)
                                               .ThenByDescending(f => f.DateAdded)
                                               .ToList();
                        var latestUpdateFileId = updateFiles.FirstOrDefault()?.Id ?? -1;
                        ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 && e.HasFile && e.RomFileId == latestUpdateFileId);
                    }

                    break;

                case MonitorTypes.AllAdditional:
                    _logger.Debug("[{0}] Monitoring all additional content (DLC + updates + future)", game.Title);
                    {
                        var files = _mediaFileService.GetFilesBySeries(game.Id);
                        var additionalFileIds = files.Where(f => f.RomFileType != RomFileType.Base || (RomReleaseType)f.RomReleaseType != RomReleaseType.Retail)
                                                    .Select(f => f.Id)
                                                    .ToHashSet();
                        ToggleGameFilesMonitoredState(roms, e => e.PlatformNumber > 0 &&
                            ((!e.AirDateUtc.HasValue || e.AirDateUtc >= DateTime.UtcNow) ||
                             (e.HasFile && additionalFileIds.Contains(e.RomFileId))));
                    }

                    break;
            }

            var monitoredPlatforms = roms.Where(e => e.Monitored)
                                           .Select(e => e.PlatformNumber)
                                           .Distinct()
                                           .ToList();

            foreach (var platform in game.Platforms)
            {
                var platformNumber = platform.PlatformNumber;

                // Monitor the last platform when:
                // - Not specials
                // - The latest platform
                // - Set to monitor all roms
                // - Set to monitor future roms and game is continuing or not yet aired
                if (platformNumber > 0 &&
                    platformNumber == lastPlatform &&
                    (monitoringOptions.Monitor == MonitorTypes.All ||
                     (monitoringOptions.Monitor == MonitorTypes.Future && game.Status is GameStatusType.Continuing or GameStatusType.Upcoming)))
                {
                    platform.Monitored = true;
                }
                else if (monitoredPlatforms.Contains(platformNumber))
                {
                    // Monitor the platform if it has any monitor roms
                    platform.Monitored = true;
                }

                // Don't monitor the platform
                else
                {
                    platform.Monitored = false;
                }
            }

            _romService.UpdateGameFiles(roms);
            _gameService.UpdateSeries(game, false);
        }

        private void LegacySetGameFileMonitoredStatus(Game game, MonitoringOptions monitoringOptions)
        {
            _logger.Debug("[{0}] Setting rom monitored status.", game.Title);

            var roms = _romService.GetGameFileBySeries(game.Id);

            if (monitoringOptions.IgnoreGameFilesWithFiles)
            {
                _logger.Debug("Unmonitoring Roms with Files");
                ToggleGameFilesMonitoredState(roms.Where(e => e.HasFile), false);
            }
            else
            {
                _logger.Debug("Monitoring Roms with Files");
                ToggleGameFilesMonitoredState(roms.Where(e => e.HasFile), true);
            }

            if (monitoringOptions.IgnoreGameFilesWithoutFiles)
            {
                _logger.Debug("Unmonitoring Roms without Files");
                ToggleGameFilesMonitoredState(roms.Where(e => !e.HasFile && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)), false);
            }
            else
            {
                _logger.Debug("Monitoring Roms without Files");
                ToggleGameFilesMonitoredState(roms.Where(e => !e.HasFile && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)), true);
            }

            var lastPlatform = game.Platforms.Select(s => s.PlatformNumber).MaxOrDefault();

            foreach (var s in game.Platforms)
            {
                var platform = s;

                // If the platform is unmonitored we should unmonitor all roms in that platform

                if (!platform.Monitored)
                {
                    _logger.Debug("Unmonitoring all roms in platform {0}", platform.PlatformNumber);
                    ToggleGameFilesMonitoredState(roms.Where(e => e.PlatformNumber == platform.PlatformNumber), false);
                }

                // If the platform is not the latest platform and all it's roms are unmonitored the platform will be unmonitored

                if (platform.PlatformNumber < lastPlatform)
                {
                    if (roms.Where(e => e.PlatformNumber == platform.PlatformNumber).All(e => !e.Monitored))
                    {
                        _logger.Debug("Unmonitoring platform {0} because all roms are not monitored", platform.PlatformNumber);
                        platform.Monitored = false;
                    }
                }
            }

            _romService.UpdateGameFiles(roms);

            _gameService.UpdateSeries(game, false);
        }

        private void ToggleGameFilesMonitoredState(IEnumerable<Rom> roms, bool monitored)
        {
            foreach (var rom in roms)
            {
                rom.Monitored = monitored;
            }
        }

        private void ToggleGameFilesMonitoredState(List<Rom> roms, Func<Rom, bool> predicate)
        {
            ToggleGameFilesMonitoredState(roms.Where(predicate), true);
            ToggleGameFilesMonitoredState(roms.Where(e => !predicate(e)), false);
        }
    }
}
