using System.Collections.Generic;
using Romarr.Common.Messaging;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;

namespace Romarr.Core.Download
{
    public class DownloadFailedEvent : IEvent
    {
        public DownloadFailedEvent()
        {
            Data = new Dictionary<string, string>();
        }

        public int GameId { get; set; }
        public List<int> RomIds { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public List<Language> Languages { get; set; }
        public bool SkipRedownload { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }
    }
}
