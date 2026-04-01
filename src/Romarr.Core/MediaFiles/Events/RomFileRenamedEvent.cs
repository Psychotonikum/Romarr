using Romarr.Common.Messaging;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.Events
{
    public class RomFileRenamedEvent : IEvent
    {
        public Game Game { get; private set; }
        public RomFile RomFile { get; private set; }
        public string OriginalPath { get; private set; }

        public RomFileRenamedEvent(Game game, RomFile romFile, string originalPath)
        {
            Game = game;
            RomFile = romFile;
            OriginalPath = originalPath;
        }
    }
}
