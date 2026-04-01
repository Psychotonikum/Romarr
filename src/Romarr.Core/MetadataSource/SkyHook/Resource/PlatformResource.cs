using System.Collections.Generic;

namespace Romarr.Core.MetadataSource.SkyHook.Resource
{
    public class PlatformResource
    {
        public PlatformResource()
        {
            Images = new List<ImageResource>();
        }

        public int PlatformNumber { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}
