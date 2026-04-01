using Newtonsoft.Json;
using Romarr.Core.Download.Clients.NzbVortex.JsonConverters;

namespace Romarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexResponseBase
    {
        [JsonConverter(typeof(NzbVortexResultTypeConverter))]
        public NzbVortexResultType Result { get; set; }
    }
}
