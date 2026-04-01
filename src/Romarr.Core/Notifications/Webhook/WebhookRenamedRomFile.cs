using Romarr.Core.MediaFiles;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookRenamedRomFile : WebhookRomFile
    {
        public WebhookRenamedRomFile(RenamedRomFile renamedGameFile)
            : base(renamedGameFile.RomFile)
        {
            PreviousRelativePath = renamedGameFile.PreviousRelativePath;
            PreviousPath = renamedGameFile.PreviousPath;
        }

        public string PreviousRelativePath { get; set; }
        public string PreviousPath { get; set; }
    }
}
