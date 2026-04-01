using System.Collections.Generic;
using System.Linq;

namespace Romarr.Core.GameStats
{
    public interface IGameStatisticsService
    {
        List<GameStatistics> GameStatistics();
        GameStatistics GameStatistics(int gameId);
    }

    public class GameStatisticsService : IGameStatisticsService
    {
        private readonly IGameStatisticsRepository _gameStatisticsRepository;

        public GameStatisticsService(IGameStatisticsRepository gameStatisticsRepository)
        {
            _gameStatisticsRepository = gameStatisticsRepository;
        }

        public List<GameStatistics> GameStatistics()
        {
            var platformStatistics = _gameStatisticsRepository.GameStatistics();

            return platformStatistics.GroupBy(s => s.GameId).Select(s => MapGameStatistics(s.ToList())).ToList();
        }

        public GameStatistics GameStatistics(int gameId)
        {
            var stats = _gameStatisticsRepository.GameStatistics(gameId);

            if (stats == null || stats.Count == 0)
            {
                return new GameStatistics();
            }

            return MapGameStatistics(stats);
        }

        private GameStatistics MapGameStatistics(List<PlatformStatistics> platformStatistics)
        {
            var gameStatistics = new GameStatistics
            {
                PlatformStatistics = platformStatistics,
                GameId = platformStatistics.First().GameId,
                RomFileCount = platformStatistics.Sum(s => s.RomFileCount),
                GameFileCount = platformStatistics.Sum(s => s.GameFileCount),
                TotalGameFileCount = platformStatistics.Sum(s => s.TotalGameFileCount),
                MonitoredGameFileCount = platformStatistics.Sum(s => s.MonitoredGameFileCount),
                SizeOnDisk = platformStatistics.Sum(s => s.SizeOnDisk),
                ReleaseGroups = platformStatistics.SelectMany(s => s.ReleaseGroups).Distinct().ToList()
            };

            var nextAiring = platformStatistics.Where(s => s.NextAiring != null).MinBy(s => s.NextAiring);
            var previousAiring = platformStatistics.Where(s => s.PreviousAiring != null).MaxBy(s => s.PreviousAiring);
            var lastAired = platformStatistics.Where(s => s.PlatformNumber > 0 && s.LastAired != null).MaxBy(s => s.LastAired);

            gameStatistics.NextAiring = nextAiring?.NextAiring;
            gameStatistics.PreviousAiring = previousAiring?.PreviousAiring;
            gameStatistics.LastAired = lastAired?.LastAired;

            return gameStatistics;
        }
    }
}
