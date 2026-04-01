using Romarr.Common.Messaging;

namespace Romarr.Core.MediaFiles.Events
{
    public class RomFileAddedEvent : IEvent
    {
        public RomFile RomFile { get; private set; }

        public RomFileAddedEvent(RomFile romFile)
        {
            RomFile = romFile;
        }
    }
}
