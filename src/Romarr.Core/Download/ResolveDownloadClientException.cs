using System.Net;
using Romarr.Core.Exceptions;

namespace Romarr.Core.Download;

public class ResolveDownloadClientException : RomarrClientException
{
    public ResolveDownloadClientException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
