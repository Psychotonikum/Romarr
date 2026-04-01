using System.Text.Json.Serialization;
using Romarr.Core.CustomFormats;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookCustomFormat
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public int Id { get; set; }
        public string Name { get; set; }

        public WebhookCustomFormat(CustomFormat customFormat)
        {
            Id = customFormat.Id;
            Name = customFormat.Name;
        }
    }
}
