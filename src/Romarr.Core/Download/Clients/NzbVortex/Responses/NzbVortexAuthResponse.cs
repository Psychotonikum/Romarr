using Newtonsoft.Json;
using Romarr.Core.Download.Clients.NzbVortex.JsonConverters;

namespace Romarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexAuthResponse : NzbVortexResponseBase
    {
        [JsonConverter(typeof(NzbVortexLoginResultTypeConverter))]
        public NzbVortexLoginResultType LoginResult { get; set; }

        public string SessionId { get; set; }
    }
}
