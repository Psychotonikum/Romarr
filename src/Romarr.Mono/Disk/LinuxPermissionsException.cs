using Romarr.Common.Exceptions;

namespace Romarr.Mono.Disk
{
    public class LinuxPermissionsException : RomarrException
    {
        public LinuxPermissionsException(string message, params object[] args)
            : base(message, args)
        {
        }

        public LinuxPermissionsException(string message)
            : base(message)
        {
        }
    }
}
