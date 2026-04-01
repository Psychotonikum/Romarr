using System;
using System.Collections.Generic;
using Romarr.Core.Datastore;
using Romarr.Core.Indexers;
using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Blocklisting
{
    public class Blocklist : ModelBase
    {
        public int GameId { get; set; }
        public Game Game { get; set; }
        public List<int> RomIds { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PublishedDate { get; set; }
        public long? Size { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string TorrentInfoHash { get; set; }
        public List<Language> Languages { get; set; }
    }
}
