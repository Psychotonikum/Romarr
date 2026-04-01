using System.Collections.Generic;

namespace Romarr.Core.Notifications.Trakt.Resource
{
    public class TraktPlatformResource
    {
        public int Number { get; set; }
        public List<TraktRomResource> Roms { get; set; }
    }
}
