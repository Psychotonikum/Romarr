using System.Collections.Generic;
using Romarr.Api.V3.Game;

namespace Romarr.Api.V3.PlatformPass
{
    public class PlatformPassGameResource
    {
        public int Id { get; set; }
        public bool? Monitored { get; set; }
        public List<PlatformResource> Platforms { get; set; }
    }
}
