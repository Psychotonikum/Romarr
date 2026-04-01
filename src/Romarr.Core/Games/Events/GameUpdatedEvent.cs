using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class GameUpdatedEvent : IEvent
    {
        public Game Game { get; private set; }

        public GameUpdatedEvent(Game game)
        {
            Game = game;
        }
    }
}
