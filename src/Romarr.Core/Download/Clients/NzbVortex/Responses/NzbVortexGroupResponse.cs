using System.Collections.Generic;

namespace Romarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexGroupResponse : NzbVortexResponseBase
    {
        public List<NzbVortexGroup> Groups { get; set; }
    }
}
