using System.Net;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Exceptions
{
    public class DownstreamException : RomarrException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public DownstreamException(HttpStatusCode statusCode, string message, params object[] args)
            : base(message, args)
        {
            StatusCode = statusCode;
        }

        public DownstreamException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
