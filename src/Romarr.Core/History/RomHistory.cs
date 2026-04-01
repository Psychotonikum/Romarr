using System;
using System.Collections.Generic;
using Romarr.Core.Datastore;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.History
{
    public class FileHistory : ModelBase
    {
        public const string DOWNLOAD_CLIENT = "downloadClient";
        public const string SERIES_MATCH_TYPE = "seriesMatchType";
        public const string RELEASE_SOURCE = "releaseSource";
        public const string RELEASE_GROUP = "releaseGroup";
        public const string SIZE = "size";
        public const string INDEXER = "indexer";

        public FileHistory()
        {
            Data = new Dictionary<string, string>();
        }

        public int FileId { get; set; }
        public int GameId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public Rom Rom { get; set; }
        public Game Game { get; set; }
        public FileHistoryEventType EventType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<Language> Languages { get; set; }
        public string DownloadId { get; set; }
    }

    public enum FileHistoryEventType
    {
        Unknown = 0,
        Grabbed = 1,
        GameFolderImported = 2,
        DownloadFolderImported = 3,
        DownloadFailed = 4,
        RomFileDeleted = 5,
        RomFileRenamed = 6,
        DownloadIgnored = 7
    }
}
