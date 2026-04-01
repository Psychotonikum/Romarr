using System;
using System.Collections.Generic;
using Romarr.Core.Datastore;

namespace Romarr.Core.GameStats
{
    public class GameStatistics : ResultSet
    {
        public int GameId { get; set; }
        public DateTime? NextAiring { get; set; }
        public DateTime? PreviousAiring { get; set; }
        public DateTime? LastAired { get; set; }
        public int RomFileCount { get; set; }
        public int GameFileCount { get; set; }
        public int TotalGameFileCount { get; set; }
        public int MonitoredGameFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }
        public List<PlatformStatistics> PlatformStatistics { get; set; }
    }
}
