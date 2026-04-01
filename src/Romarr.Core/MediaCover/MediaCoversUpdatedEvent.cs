using Romarr.Common.Messaging;
using Romarr.Core.Games;

namespace Romarr.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Game Game { get; set; }
        public bool Updated { get; set; }

        public MediaCoversUpdatedEvent(Game game, bool updated)
        {
            Game = game;
            Updated = updated;
        }
    }
}
