using Romarr.Common.Messaging;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public CommandModel Command { get; private set; }

        public CommandExecutedEvent(CommandModel command)
        {
            Command = command;
        }
    }
}
