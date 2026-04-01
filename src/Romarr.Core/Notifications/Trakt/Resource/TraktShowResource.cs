using System.Collections.Generic;
namespace Romarr.Core.Notifications.Trakt.Resource
{
    public class TraktShowResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public TraktShowIdsResource Ids { get; set; }
        public List<TraktPlatformResource> Platforms { get; set; }
    }
}
