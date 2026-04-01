using System.Collections.Generic;
using Romarr.Common.Messaging;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;

namespace Romarr.Core.Download
{
    public class DownloadIgnoredEvent : IEvent
    {
        public int GameId { get; set; }
        public List<int> RomIds { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public string Message { get; set; }
    }
}
