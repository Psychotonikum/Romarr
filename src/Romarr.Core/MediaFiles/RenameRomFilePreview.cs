using System.Collections.Generic;

namespace Romarr.Core.MediaFiles
{
    public class RenameRomFilePreview
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public List<int> RomNumbers { get; set; }
        public int RomFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }
}
