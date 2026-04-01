using Romarr.Common.Exceptions;

namespace Romarr.Core.Update
{
    public class UpdateFailedException : RomarrException
    {
        public UpdateFailedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UpdateFailedException(string message)
            : base(message)
        {
        }
    }
}
