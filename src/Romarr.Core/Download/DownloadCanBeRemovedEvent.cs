using Romarr.Common.Messaging;
using Romarr.Core.Download.TrackedDownloads;

namespace Romarr.Core.Download
{
    public class DownloadCanBeRemovedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public DownloadCanBeRemovedEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
