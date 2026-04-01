using System.Collections.Generic;

namespace Romarr.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetTorrents
    {
        public Dictionary<int, BroadcastheNetTorrent> Torrents { get; set; }
        public int Results { get; set; }
    }
}
