using System.Net;

namespace Romarr.Common.Http
{
    public class BasicNetworkCredential : NetworkCredential
    {
        public BasicNetworkCredential(string user, string pass)
        : base(user, pass)
        {
        }
    }
}
