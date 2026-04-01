using System;
using System.Collections.Generic;
using Romarr.Core.Datastore;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Indexers;
using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Queue
{
    public class Queue : ModelBase
    {
        public Game Game { get; set; }

        public int? PlatformNumber { get; set; }

        [Obsolete]
        public Rom Rom { get; set; }

        public List<Rom> Roms { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }
        public decimal SizeLeft { get; set; }
        public TimeSpan? TimeLeft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string DownloadId { get; set; }
        public RemoteRom RemoteRom { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }
        public string ErrorMessage { get; set; }
    }
}
