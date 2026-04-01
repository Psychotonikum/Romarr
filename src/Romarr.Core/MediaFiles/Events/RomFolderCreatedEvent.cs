using Romarr.Common.Messaging;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.Events
{
    public class GameFileFolderCreatedEvent : IEvent
    {
        public Game Game { get; private set; }
        public RomFile RomFile { get; private set; }
        public string GameFolder { get; set; }
        public string PlatformFolder { get; set; }
        public string GameFileFolder { get; set; }

        public GameFileFolderCreatedEvent(Game game, RomFile romFile)
        {
            Game = game;
            RomFile = romFile;
        }
    }
}
