using Romarr.Core.Notifications.Trakt.Resource;

namespace Romarr.Core.Notifications.Trakt
{
    public class TraktUserResource
    {
        public string Username { get; set; }
        public TraktUserIdsResource Ids { get; set; }
    }
}
