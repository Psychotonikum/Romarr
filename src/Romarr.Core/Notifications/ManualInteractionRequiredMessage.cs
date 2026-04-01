using Romarr.Core.Download;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Notifications
{
    public class ManualInteractionRequiredMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public RemoteRom Rom { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public QualityModel Quality { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
