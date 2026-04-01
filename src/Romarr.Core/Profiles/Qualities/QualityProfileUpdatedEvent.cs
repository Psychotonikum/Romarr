using Romarr.Common.Messaging;

namespace Romarr.Core.Profiles.Qualities;

public class QualityProfileUpdatedEvent(int id) : IEvent
{
    public int Id { get; private set; } = id;
}
