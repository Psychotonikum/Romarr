using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class GameEditedEvent : IEvent
    {
        public Game Game { get; private set; }
        public Game OldSeries { get; private set; }
        public bool GameFilesChanged { get; private set; }

        public GameEditedEvent(Game game, Game oldSeries, bool gameFilesChanged = false)
        {
            Game = game;
            OldSeries = oldSeries;
            GameFilesChanged = gameFilesChanged;
        }
    }
}
