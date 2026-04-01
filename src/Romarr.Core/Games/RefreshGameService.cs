using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Common.Instrumentation.Extensions;
using Romarr.Core.Configuration;
using Romarr.Core.Exceptions;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.MetadataSource;
using Romarr.Core.Games.Commands;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public class RefreshGameService : IExecute<RefreshGameCommand>
    {
        private readonly IProvideGameInfo _gameInfo;
        private readonly IGameService _gameService;
        private readonly IRefreshRomService _refreshRomService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfSeriesShouldBeRefreshed _checkIfGameShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly ICommandResultReporter _commandResultReporter;
        private readonly Logger _logger;

        public RefreshGameService(IProvideGameInfo seriesInfo,
                                    IGameService seriesService,
                                    IRefreshRomService refreshRomService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfSeriesShouldBeRefreshed checkIfSeriesShouldBeRefreshed,
                                    IConfigService configService,
                                    ICommandResultReporter commandResultReporter,
                                    Logger logger)
        {
            _gameInfo = seriesInfo;
            _gameService = seriesService;
            _refreshRomService = refreshRomService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfGameShouldBeRefreshed = checkIfSeriesShouldBeRefreshed;
            _configService = configService;
            _commandResultReporter = commandResultReporter;
            _logger = logger;
        }

        private Game RefreshGameInfo(int gameId)
        {
            // Get the game before updating, that way any changes made to the game after the refresh started,
            // but before this game was refreshed won't be lost.
            var game = _gameService.GetGame(gameId);

            // Skip IGDB lookup for locally-imported games (negative IgdbId)
            if (game.IgdbId < 0)
            {
                _logger.Debug("Skipping IGDB refresh for locally-imported game {0} (IgdbId: {1})", game.Title, game.IgdbId);
                return game;
            }

            _logger.ProgressInfo("Updating {0}", game.Title);

            Game seriesInfo;
            List<Rom> roms;

            try
            {
                var tuple = _gameInfo.GetGameInfo(game.IgdbId);
                seriesInfo = tuple.Item1;
                roms = tuple.Item2;
            }
            catch (GameNotFoundException)
            {
                if (game.Status != GameStatusType.Deleted)
                {
                    game.Status = GameStatusType.Deleted;
                    _gameService.UpdateSeries(game, publishUpdatedEvent: false);
                    _logger.Debug("Game marked as deleted on igdb for {0}", game.Title);
                    _eventAggregator.PublishEvent(new GameUpdatedEvent(game));
                }

                throw;
            }

            if (game.IgdbId != seriesInfo.IgdbId)
            {
                _logger.Warn("Game '{0}' (igdbid {1}) was replaced with '{2}' (igdbid {3}), because the original was a duplicate.", game.Title, game.IgdbId, seriesInfo.Title, seriesInfo.IgdbId);
                game.IgdbId = seriesInfo.IgdbId;
            }

            game.Title = seriesInfo.Title;
            game.Year = seriesInfo.Year;
            game.TitleSlug = seriesInfo.TitleSlug;
            game.MobyGamesId = seriesInfo.MobyGamesId;
            game.RawgId = seriesInfo.RawgId;
            game.TmdbId = seriesInfo.TmdbId;
            game.ImdbId = seriesInfo.ImdbId;
            game.MalIds = seriesInfo.MalIds;
            game.AniListIds = seriesInfo.AniListIds;
            game.AirTime = seriesInfo.AirTime;
            game.Overview = seriesInfo.Overview;
            game.OriginalLanguage = seriesInfo.OriginalLanguage;
            game.Status = seriesInfo.Status;
            game.CleanTitle = seriesInfo.CleanTitle;
            game.SortTitle = seriesInfo.SortTitle;
            game.LastInfoSync = DateTime.UtcNow;
            game.Runtime = seriesInfo.Runtime;
            game.Images = seriesInfo.Images;
            game.Network = seriesInfo.Network;
            game.FirstAired = seriesInfo.FirstAired;
            game.LastAired = seriesInfo.LastAired;
            game.Ratings = seriesInfo.Ratings;
            game.Actors = seriesInfo.Actors;
            game.Genres = seriesInfo.Genres;
            game.Certification = seriesInfo.Certification;
            game.OriginalCountry = seriesInfo.OriginalCountry;

            try
            {
                game.Path = new DirectoryInfo(game.Path).FullName;
                game.Path = game.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update game path for " + game.Path);
            }

            game.Platforms = UpdatePlatforms(game, seriesInfo);

            _gameService.UpdateSeries(game, publishUpdatedEvent: false);
            _refreshRomService.RefreshRomInfo(game, roms);

            _logger.Debug("Finished game refresh for {0}", game.Title);
            _eventAggregator.PublishEvent(new GameUpdatedEvent(game));

            return game;
        }

        private List<Platform> UpdatePlatforms(Game game, Game seriesInfo)
        {
            var platforms = seriesInfo.Platforms.DistinctBy(s => s.PlatformNumber).ToList();

            foreach (var platform in platforms)
            {
                var existingPlatform = game.Platforms.FirstOrDefault(s => s.PlatformNumber == platform.PlatformNumber);

                if (existingPlatform == null)
                {
                    if (platform.PlatformNumber == 0)
                    {
                        _logger.Debug("Ignoring platform 0 for game [{0}] {1} by default", game.IgdbId, game.Title);
                        platform.Monitored = false;
                        continue;
                    }

                    var monitorNewPlatforms = game.MonitorNewItems == NewItemMonitorTypes.All;

                    _logger.Debug("New platform ({0}) for game: [{1}] {2}, setting monitored to {3}", platform.PlatformNumber, game.IgdbId, game.Title, monitorNewPlatforms.ToString().ToLowerInvariant());
                    platform.Monitored = monitorNewPlatforms;
                }
                else
                {
                    platform.Monitored = existingPlatform.Monitored;
                }
            }

            return platforms;
        }

        private void RescanGame(Game game, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;

            if (isNew)
            {
                _logger.Trace("Forcing rescan of {0}. Reason: New game", game);
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: never rescan after refresh", game);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(game, SeriesScanSkippedReason.NeverRescanAfterRefresh));

                return;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: not after automatic scans", game);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(game, SeriesScanSkippedReason.RescanAfterManualRefreshOnly));

                return;
            }

            try
            {
                _diskScanService.Scan(game);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan game {0}", game);
            }
        }

        private void UpdateTags(Game game)
        {
            var tagsUpdated = _gameService.UpdateTags(game);

            if (tagsUpdated)
            {
                _gameService.UpdateSeries(game);
            }
        }

        public void Execute(RefreshGameCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewSeries;
            _eventAggregator.PublishEvent(new GameRefreshStartingEvent(trigger == CommandTrigger.Manual));

            if (message.GameIds.Any())
            {
                foreach (var gameId in message.GameIds)
                {
                    var game = _gameService.GetGame(gameId);

                    try
                    {
                        game = RefreshGameInfo(gameId);
                        UpdateTags(game);
                        RescanGame(game, isNew, trigger);
                    }
                    catch (GameNotFoundException)
                    {
                        _logger.Error("Game '{0}' (igdbid {1}) was not found, it may have been removed from TheIGDB.", game.Title, game.IgdbId);

                        // Mark the result as indeterminate so it's not marked as a full success,
                        // // but we can still process other game if needed.
                        _commandResultReporter.Report(CommandResult.Indeterminate);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", game);
                        UpdateTags(game);
                        RescanGame(game, isNew, trigger);

                        // Mark the result as indeterminate so it's not marked as a full success,
                        // but we can still process other game if needed.
                        _commandResultReporter.Report(CommandResult.Indeterminate);
                    }
                }
            }
            else
            {
                var allGames = _gameService.GetAllGames().OrderBy(c => c.SortTitle).ToList();

                foreach (var game in allGames)
                {
                    var seriesLocal = game;
                    if (trigger == CommandTrigger.Manual || _checkIfGameShouldBeRefreshed.ShouldRefresh(seriesLocal))
                    {
                        try
                        {
                            seriesLocal = RefreshGameInfo(seriesLocal.Id);
                        }
                        catch (GameNotFoundException)
                        {
                            _logger.Error("Game '{0}' (igdbid {1}) was not found, it may have been removed from TheIGDB.", seriesLocal.Title, seriesLocal.IgdbId);
                            continue;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", seriesLocal);
                        }

                        UpdateTags(game);
                        RescanGame(seriesLocal, false, trigger);
                    }
                    else
                    {
                        _logger.Info("Skipping refresh of game: {0}", seriesLocal.Title);
                        UpdateTags(game);
                        RescanGame(seriesLocal, false, trigger);
                    }
                }
            }

            _eventAggregator.PublishEvent(new GameRefreshCompleteEvent());
        }
    }
}
