using System.Net;
using Romarr.Core.Exceptions;

namespace Romarr.Core.Backup
{
    public class RestoreBackupFailedException : RomarrClientException
    {
        public RestoreBackupFailedException(HttpStatusCode statusCode, string message, params object[] args)
            : base(statusCode, message, args)
        {
        }

        public RestoreBackupFailedException(HttpStatusCode statusCode, string message)
            : base(statusCode, message)
        {
        }
    }
}
