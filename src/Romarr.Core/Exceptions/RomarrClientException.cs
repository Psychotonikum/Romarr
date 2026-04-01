using System;
using System.Net;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Exceptions
{
    public class RomarrClientException : RomarrException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public RomarrClientException(HttpStatusCode statusCode, string message, params object[] args)
            : base(message, args)
        {
            StatusCode = statusCode;
        }

        public RomarrClientException(HttpStatusCode statusCode, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
            StatusCode = statusCode;
        }

        public RomarrClientException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
