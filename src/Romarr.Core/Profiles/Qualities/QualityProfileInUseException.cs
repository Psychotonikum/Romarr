using System.Net;
using Romarr.Core.Exceptions;

namespace Romarr.Core.Profiles.Qualities
{
    public class QualityProfileInUseException : RomarrClientException
    {
        public QualityProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "QualityProfile [{0}] is in use.", name)
        {
        }
    }
}
