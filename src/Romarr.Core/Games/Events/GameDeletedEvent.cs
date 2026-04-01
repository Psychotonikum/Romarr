using System.Collections.Generic;
using Romarr.Common.Messaging;

namespace Romarr.Core.Games.Events
{
    public class GameDeletedEvent : IEvent
    {
        public List<Game> Game { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public GameDeletedEvent(List<Game> game, bool deleteFiles, bool addImportListExclusion)
        {
            Game = game;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
