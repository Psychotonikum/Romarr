using System.Net;
using Romarr.Core.Exceptions;

namespace Romarr.Core.MetadataSource.SkyHook;

public class InvalidSearchTermException : RomarrClientException
{
    public InvalidSearchTermException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
