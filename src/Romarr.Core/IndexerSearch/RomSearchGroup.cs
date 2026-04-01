using System.Collections.Generic;
using Romarr.Core.Games;

namespace Romarr.Core.IndexerSearch
{
    public class RomSearchGroup
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public List<Rom> Roms { get; set; }
    }
}
