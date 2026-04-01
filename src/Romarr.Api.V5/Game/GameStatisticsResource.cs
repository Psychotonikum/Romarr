using Romarr.Core.GameStats;

namespace Romarr.Api.V5.Game;

public class GameStatisticsResource
{
    public int PlatformCount { get; set; }
    public int RomFileCount { get; set; }
    public int GameFileCount { get; set; }
    public int TotalGameFileCount { get; set; }
    public int MonitoredGameFileCount { get; set; }
    public long SizeOnDisk { get; set; }
    public List<string>? ReleaseGroups { get; set; }

    public decimal PercentOfGameFiles
    {
        get
        {
            if (GameFileCount == 0)
            {
                return 0;
            }

            return (decimal)RomFileCount / (decimal)GameFileCount * 100;
        }
    }
}

public static class GameStatisticsResourceMapper
{
    public static GameStatisticsResource ToResource(this GameStatistics model, List<PlatformResource>? platforms)
    {
        return new GameStatisticsResource
        {
            PlatformCount = platforms?.Count(s => s.PlatformNumber > 0) ?? 0,
            RomFileCount = model.RomFileCount,
            GameFileCount = model.GameFileCount,
            TotalGameFileCount = model.TotalGameFileCount,
            MonitoredGameFileCount = model.MonitoredGameFileCount,
            SizeOnDisk = model.SizeOnDisk,
            ReleaseGroups = model.ReleaseGroups
        };
    }
}
