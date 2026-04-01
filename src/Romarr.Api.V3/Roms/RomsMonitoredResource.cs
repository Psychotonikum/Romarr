using System.Collections.Generic;

namespace Romarr.Api.V3.Roms
{
    public class FilesMonitoredResource
    {
        public List<int> RomIds { get; set; }
        public bool Monitored { get; set; }
    }
}
