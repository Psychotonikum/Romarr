using Romarr.Common.Messaging;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Download
{
    public class ManualInteractionRequiredEvent : IEvent
    {
        public RemoteRom Rom { get; private set; }
        public TrackedDownload TrackedDownload { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public ManualInteractionRequiredEvent(TrackedDownload trackedDownload, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            Rom = trackedDownload.RemoteRom;
            Release = release;
        }
    }
}
