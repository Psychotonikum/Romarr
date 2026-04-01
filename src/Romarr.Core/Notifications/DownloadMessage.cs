using System.Collections.Generic;
using Romarr.Core.Download;
using Romarr.Core.MediaFiles;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.Notifications
{
    public class DownloadMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public LocalGameFile RomInfo { get; set; }
        public RomFile RomFile { get; set; }
        public List<DeletedRomFile> OldFiles { get; set; }
        public string SourcePath { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
