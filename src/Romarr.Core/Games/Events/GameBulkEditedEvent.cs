using System.Collections.Generic;
using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class SeriesBulkEditedEvent : IEvent
    {
        public List<Game> Game { get; private set; }

        public SeriesBulkEditedEvent(List<Game> game)
        {
            Game = game;
        }
    }
}
