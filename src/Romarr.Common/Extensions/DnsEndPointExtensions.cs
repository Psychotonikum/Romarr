using System.Net;

namespace Romarr.Common.Extensions
{
    public static class DnsEndPointExtensions
    {
        extension(DnsEndPoint endPoint)
        {
            public string HostPort => $"{endPoint.Host}:{endPoint.Port}";
        }
    }
}
