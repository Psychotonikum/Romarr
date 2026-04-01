using System.Collections.Generic;
using Romarr.Core.MediaFiles;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookGameFileDeletePayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookGameFile> Roms { get; set; }
        public WebhookRomFile RomFile { get; set; }
        public DeleteMediaFileReason DeleteReason { get; set; }
    }
}
