using System.Net;

namespace Romarr.Core.Exceptions
{
    public class BadRequestException : DownstreamException
    {
        public BadRequestException(string message)
            : base(HttpStatusCode.BadRequest, message)
        {
        }

        public BadRequestException(string message, params object[] args)
            : base(HttpStatusCode.BadRequest, message, args)
        {
        }
    }
}
