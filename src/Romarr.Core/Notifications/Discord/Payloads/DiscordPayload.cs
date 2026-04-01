using System.Collections.Generic;
using Newtonsoft.Json;

namespace Romarr.Core.Notifications.Discord.Payloads
{
    public class DiscordPayload
    {
        public string Content { get; set; }

        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        public List<Embed> Embeds { get; set; }
    }
}
