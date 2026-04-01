using Romarr.Common.Exceptions;

namespace Romarr.Core.Messaging.Commands
{
    public class CommandNotFoundException : RomarrException
    {
        public CommandNotFoundException(string contract)
            : base("Couldn't find command " + contract)
        {
        }
    }
}
