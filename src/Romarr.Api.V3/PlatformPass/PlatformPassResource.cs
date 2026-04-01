using System.Collections.Generic;
using Romarr.Core.Games;

namespace Romarr.Api.V3.PlatformPass
{
    public class PlatformPassResource
    {
        public List<PlatformPassGameResource> Game { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
