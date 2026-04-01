using System.Net;
using Romarr.Core.Exceptions;

namespace Romarr.Core.Indexers;

public class ResolveIndexerException : RomarrClientException
{
    public ResolveIndexerException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
