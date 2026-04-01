using System.Collections.Generic;
using Romarr.Common.Messaging;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.Events
{
    public class SeriesScannedEvent : IEvent
    {
        public Game Game { get; private set; }
        public List<string> PossibleExtraFiles { get; set; }

        public SeriesScannedEvent(Game game, List<string> possibleExtraFiles)
        {
            Game = game;
            PossibleExtraFiles = possibleExtraFiles;
        }
    }
}
