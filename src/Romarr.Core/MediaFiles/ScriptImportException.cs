using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.MediaFiles
{
    public class ScriptImportException : RomarrException
    {
        public ScriptImportException(string message)
            : base(message)
        {
        }

        public ScriptImportException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ScriptImportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
