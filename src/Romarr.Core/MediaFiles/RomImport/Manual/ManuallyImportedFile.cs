using Romarr.Core.Download.TrackedDownloads;

namespace Romarr.Core.MediaFiles.GameFileImport.Manual
{
    public class ManuallyImportedFile
    {
        public TrackedDownload TrackedDownload { get; set; }
        public ImportResult ImportResult { get; set; }
    }
}
