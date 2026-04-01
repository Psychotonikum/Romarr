using System.Collections.Generic;
using System.Linq;
using Romarr.Core.CustomFormats;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookCustomFormatInfo
    {
        public List<WebhookCustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }

        public WebhookCustomFormatInfo(List<CustomFormat> customFormats, int customFormatScore)
        {
            CustomFormats = customFormats.Select(c => new WebhookCustomFormat(c)).ToList();
            CustomFormatScore = customFormatScore;
        }
    }
}
