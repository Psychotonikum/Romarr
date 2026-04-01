using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class SeriesAddCompletedEvent : IEvent
    {
        public Game Game { get; private set; }

        public SeriesAddCompletedEvent(Game game)
        {
            Game = game;
        }
    }
}
