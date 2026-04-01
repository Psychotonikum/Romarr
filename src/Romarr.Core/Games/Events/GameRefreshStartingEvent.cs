using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class GameRefreshStartingEvent : IEvent
    {
        public bool ManualTrigger { get; set; }

        public GameRefreshStartingEvent(bool manualTrigger)
        {
            ManualTrigger = manualTrigger;
        }
    }
}
