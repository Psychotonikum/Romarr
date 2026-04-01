using Romarr.Core.MediaFiles.GameFileImport;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.MediaFiles.Commands
{
    public class DownloadedGameFilesScanCommand : Command
    {
        // Properties used by third-party apps, do not modify.
        public string Path { get; set; }
        public string DownloadClientId { get; set; }
        public ImportMode ImportMode { get; set; }
        public override bool RequiresDiskAccess => true;
        public override bool IsLongRunning => true;
    }
}
