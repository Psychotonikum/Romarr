using Romarr.Common.Messaging;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Download
{
    public class FileGrabbedEvent : IEvent
    {
        public RemoteRom Rom { get; private set; }
        public int DownloadClientId { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public FileGrabbedEvent(RemoteRom rom)
        {
            Rom = rom;
        }
    }
}
