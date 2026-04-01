using System.Collections.Generic;
using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class GameImportedEvent : IEvent
    {
        public List<int> GameIds { get; private set; }

        public GameImportedEvent(List<int> gameIds)
        {
            GameIds = gameIds;
        }
    }
}
