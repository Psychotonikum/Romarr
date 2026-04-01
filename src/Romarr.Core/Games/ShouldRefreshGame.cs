using System;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;

namespace Romarr.Core.Games
{
    public interface ICheckIfSeriesShouldBeRefreshed
    {
        bool ShouldRefresh(Game game);
    }

    public class ShouldRefreshGame : ICheckIfSeriesShouldBeRefreshed
    {
        private readonly IRomService _romService;
        private readonly Logger _logger;

        public ShouldRefreshGame(IRomService gameFileService, Logger logger)
        {
            _romService = gameFileService;
            _logger = logger;
        }

        public bool ShouldRefresh(Game game)
        {
            try
            {
                if (game.LastInfoSync < DateTime.UtcNow.AddDays(-30))
                {
                    _logger.Trace("Game {0} last updated more than 30 days ago, should refresh.", game.Title);
                    return true;
                }

                var roms = _romService.GetGameFileBySeries(game.Id);

                var atLeastOneAiredGameFileWithoutTitle = roms.Any(e =>
                    e.PlatformNumber > 0 &&
                    e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow) &&
                    e.Title.Equals("TBA", StringComparison.Ordinal));

                if (atLeastOneAiredGameFileWithoutTitle)
                {
                    _logger.Trace("Game {0} with at least one aired rom with TBA title, should refresh.",
                        game.Title);
                    return true;
                }

                if (game.LastInfoSync >= DateTime.UtcNow.AddHours(-6))
                {
                    _logger.Trace("Game {0} last updated less than 6 hours ago, should not be refreshed.",
                        game.Title);
                    return false;
                }

                if (game.Status != GameStatusType.Ended)
                {
                    _logger.Trace("Game {0} is not ended, should refresh.", game.Title);
                    return true;
                }

                var lastGameFile = roms.MaxBy(e => e.AirDateUtc);

                if (lastGameFile != null && lastGameFile.AirDateUtc > DateTime.UtcNow.AddDays(-30))
                {
                    _logger.Trace("Last rom in {0} aired less than 30 days ago, should refresh.", game.Title);
                    return true;
                }

                _logger.Trace("Game {0} ended long ago, should not be refreshed.", game.Title);
                return false;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to determine if game should refresh, will try to refresh.");
                return true;
            }
        }
    }
}
