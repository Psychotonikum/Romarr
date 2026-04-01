using System.Collections.Generic;
using Romarr.Common.Messaging;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.MediaFiles;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Download
{
    public class DownloadCompletedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }
        public int GameId { get; private set; }
        public List<RomFile> RomFiles { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public DownloadCompletedEvent(TrackedDownload trackedDownload, int gameId, List<RomFile> romFiles, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            GameId = gameId;
            RomFiles = romFiles;
            Release = release;
        }
    }
}
