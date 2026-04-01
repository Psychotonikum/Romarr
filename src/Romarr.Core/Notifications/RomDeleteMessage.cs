using Romarr.Core.MediaFiles;
using Romarr.Core.Games;

namespace Romarr.Core.Notifications
{
    public class GameFileDeleteMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public RomFile RomFile { get; set; }

        public DeleteMediaFileReason Reason { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
