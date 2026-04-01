using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Configuration
{
    public class InvalidConfigFileException : RomarrException
    {
        public InvalidConfigFileException(string message)
            : base(message)
        {
        }

        public InvalidConfigFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
