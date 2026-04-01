using System.Collections.Generic;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookRenamePayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookRenamedRomFile> RenamedRomFiles { get; set; }
    }
}
