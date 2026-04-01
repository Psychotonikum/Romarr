using System.Collections.Generic;
using Romarr.Common.Messaging;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.Events
{
    public class FileImportedEvent : IEvent
    {
        public LocalGameFile RomInfo { get; private set; }
        public RomFile ImportedGameFile { get; private set; }
        public List<DeletedRomFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; private set; }

        public FileImportedEvent(LocalGameFile romInfo, RomFile importedGameFile, List<DeletedRomFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            RomInfo = romInfo;
            ImportedGameFile = importedGameFile;
            OldFiles = oldFiles;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClientInfo = downloadClientItem.DownloadClientInfo;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
