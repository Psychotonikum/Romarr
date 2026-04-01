using System;
using Romarr.Common.Exceptions;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Exceptions
{
    public class ReleaseDownloadException : RomarrException
    {
        public ReleaseInfo Release { get; set; }

        public ReleaseDownloadException(ReleaseInfo release, string message, params object[] args)
            : base(message, args)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, string message)
            : base(message)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, string message, Exception innerException)
            : base(message, innerException)
        {
            Release = release;
        }
    }
}
