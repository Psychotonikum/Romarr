using System;
using Romarr.Common.Messaging;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.Events
{
    public class GameFileImportFailedEvent : IEvent
    {
        public Exception Exception { get; set; }
        public LocalGameFile RomInfo { get; }
        public bool NewDownload { get; }
        public DownloadClientItemClientInfo DownloadClientInfo { get;  }
        public string DownloadId { get; }

        public GameFileImportFailedEvent(Exception exception, LocalGameFile romInfo, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Exception = exception;
            RomInfo = romInfo;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClientInfo = downloadClientItem.DownloadClientInfo;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
