using Romarr.Common.Messaging;

namespace Romarr.Core.Lifecycle
{
    [LifecycleEvent]
    public class ApplicationShutdownRequested : IEvent
    {
        public bool Restarting { get; }

        public ApplicationShutdownRequested(bool restarting = false)
        {
            Restarting = restarting;
        }
    }
}
