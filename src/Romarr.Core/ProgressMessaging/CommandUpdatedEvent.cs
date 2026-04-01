using Romarr.Common.Messaging;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.ProgressMessaging
{
    public class CommandUpdatedEvent : IEvent
    {
        public CommandModel Command { get; set; }

        public CommandUpdatedEvent(CommandModel command)
        {
            Command = command;
        }
    }
}
